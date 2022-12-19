using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackAnalyzer
{
    public class CompilationEngine
    {
        private JackTokenizer tokenizer;
        private StreamWriter output;
        private bool expectingLastToken = false;

        public CompilationEngine(StreamReader input, StreamWriter output)
        {
            tokenizer = new JackTokenizer(input);    
            this.output = output;
        }

        private Token ct => tokenizer.CurrentToken ?? throw new Exception("Why is the current token null?");        
        
        public void CompileClass()
        {
            // There is no initial token.
            Advance();

            WriteStartElement("class");
            ProcessKeyword(Keyword.Class);
            ProcessIdentifier(description: "class name");
            ProcessSymbol("{");
          
            while (CurrentTokenIsClassVarDec)
            {
                CompileClassVarDec();
            }

            while (CurrentTokenIsSubroutineDec)
            {
                CompileSubroutineDec();
            }

            expectingLastToken = true;
            ProcessSymbol("}");
            WriteEndElement("class");            
        }

        private void CompileClassVarDec()
        {
            WriteStartElement("classVarDec");
            ProcessOneOf(Keyword.Static, Keyword.Field);

            if (CurrentTokenIsTypename == false)
                throw new Exception($"Expected typename (int/char/bool/classname). Current token: {tokenizer.CurrentToken}.");

            Process(); // typename
            ProcessIdentifier(description: "variable name");

            while(CurrentTokenIsComma)
            {
                ProcessSymbol(",");
                ProcessIdentifier("variable name");
            }

            ProcessSymbol(";");
            WriteEndElement("classVarDec");
        }

        private void CompileSubroutineDec()
        {
            WriteStartElement("subroutineDec");
            ProcessOneOf(Keyword.Constructor, Keyword.Function, Keyword.Method);

            if (ct.TokenType == TokenType.Identifier ||
               (ct.TokenType == TokenType.Keyword && ct.Keyword == Keyword.Void))
            {
                Process();
            }
            else
            {
                throw new Exception($"Expected identifier or void. Current token: {tokenizer.CurrentToken}.");
            }

            ProcessIdentifier(description: "subroutine name");
            ProcessSymbol("(");

            while(CurrentTokenIsTypename)
            {
                ProcessTypename();
                ProcessIdentifier(description: "variable name");
                if(CurrentTokenIsComma)
                {
                    Process(); // comma
                    continue; // I'm going to cheat and allow a trailing comma
                }

                break;
            }


            ProcessSymbol(")");

            //begin body
            ProcessSymbol("{");

            while(ct.Keyword == Keyword.Var)
            {
                Process(); // var keyword
                ProcessTypename();
                ProcessIdentifier("variable name");
                while(CurrentTokenIsComma)
                {
                    Process();
                    ProcessIdentifier("variable name");
                }

                ProcessSymbol(";");
            }

            CompileStatements();
            
            //TODO: statements            

            ProcessSymbol("}");
            WriteEndElement("subroutineDec");
        }

        public void CompileStatements()
        {
            while (CurrentTokenIsStatement)
            {
                switch(ct.Keyword)
                {
                    case Keyword.Let:
                        Process(); // let keyword
                        ProcessIdentifier("variable name");
                        if(ct.TokenType == TokenType.Symbol && ct.Value == "[")
                        {
                            Process(); // the opening bracket
                            CompileExpression();                            
                            ProcessSymbol("]");
                        }

                        ProcessSymbol("=");
                        CompileExpression();
                        ProcessSymbol(";");
                        break;
                    case Keyword.If:
                        Process(); // if keyword
                        ProcessSymbol("(");
                        CompileExpression();
                        ProcessSymbol(")");
                        ProcessSymbol("{");
                        CompileStatements();
                        ProcessSymbol("}");

                        if(ct.Keyword == Keyword.Else)
                        {
                            Process(); // else keyword
                            ProcessSymbol("{");
                            CompileStatements();
                            ProcessSymbol("}");
                        }
                        break;
                    case Keyword.While:
                        Process(); // while keyword
                        ProcessSymbol("(");
                        CompileExpression();
                        ProcessSymbol(")");
                        ProcessSymbol("{");
                        CompileStatements();
                        ProcessSymbol("}");
                        break;
                    case Keyword.Do:
                        Process(); // do keyword
                        //ProcessSubroutineCall();
                        CompileTerm();
                        ProcessSymbol(";");
                        break;
                    case Keyword.Return:
                        Process(); // return keyword
                        if(ct.TokenType == TokenType.Symbol && ct.Value == ";")
                        {
                            ProcessSymbol(";");
                        }
                        else
                        {
                            CompileExpression();
                            ProcessSymbol(";");
                        }
                                                
                        break;
                    default:
                        throw new Exception("unreachable");
                }
            }
        }

        public void CompileTerm()
        {
            if (ct.TokenType == TokenType.IntegerConstant)
            {
                Process();
            }
            else if (ct.TokenType == TokenType.StringConstant)
            {
                Process();
            }
            else if (CurrentTokenIsKeywordConstant)
            {
                Process();
            }
            else if (CurrentTokenIsOpenParen)
            {
                ProcessSymbol("(");
                CompileExpression();
                ProcessSymbol(")");
            }
            else if (CurrentTokenIsUnaryOp)
            {
                Process();
                CompileTerm();
            }
            else if (ct.TokenType == TokenType.Identifier)
            {
                var identifier = ct;
                Advance();

                if (ct.TokenType == TokenType.Symbol)
                {
                    switch(ct.Value)
                    {
                        case "(":
                            WriteXml(identifier); // subroutine name
                            ProcessParameterList();
                            break;
                        case "[":
                            WriteXml(identifier); // variable name
                            ProcessSymbol("[");
                            CompileExpression();
                            ProcessSymbol("]");
                            break;
                        case ".":
                            WriteXml(identifier); // class name or variable name
                            ProcessSymbol(".");
                            ProcessIdentifier("subroutine name");
                            ProcessParameterList();
                            break;
                        default:
                            WriteXml(identifier); // variable name
                            break;
                    }
                }
            }
            else
            {
                throw new Exception($"Expected a term, got '{ct}'.");
            }


            
        }

        private void CompileExpression()
        {
            CompileTerm();
            while (CurrentTokenIsOp)
            {
                Process(); // the operator
                CompileTerm();
            }
        }

        private void Advance()
        {
            if (tokenizer.HasMoreTokens)
                tokenizer.Advance();
            else
            {
                if(expectingLastToken == false)
                    throw new Exception("Unexpected end of token stream");
            }                
        }

        private void WriteStartElement(string element) => output.WriteLine($"<{element}>");
        private void WriteEndElement(string element) => output.WriteLine($"</{element}>");

        private void WriteXml(Token token)
        {
            string elementName = token.TokenType.ToString().ToLower();
            output.WriteLine($"<{elementName}> {token.Value} </{elementName}>");
        }

        private void Process() => Process(ct);
        private void Process(Token token)
        {
            WriteXml(token);
            Advance();
        }

        private void Process(TokenType tokenType, string? expectedValue = null, string? description = null)
        {
            Process(ct, tokenType, expectedValue, description);
        }

        private void Process(Token theToken, TokenType tokenType, string? expectedValue = null, string? description = null)
        {
            if (theToken.TokenType == tokenType && (expectedValue == null || theToken.Value == expectedValue))
            {
                Process();
            }
            else
            {
                throw new Exception($"Expected a {description ?? ""} which should be a token of type '{tokenType}' with value '{expectedValue ?? "we dont care"}'. Current token: '{theToken}'.");
            }
        }

        private void ProcessKeyword(Keyword keyword)
        {
            if (ct.TokenType == TokenType.Keyword && ct.Keyword == keyword)
            {
                Process();
            }
            else
            {
                throw new Exception($"Expected keyword: '{keyword}'. Current token: '{ct}'.");
            }
        }

        private void ProcessSymbol(string expectedValue)
        {
            Process(TokenType.Symbol, expectedValue: expectedValue);
        }

        private void ProcessIdentifier(string description)
        {
            Process(TokenType.Identifier, expectedValue: null, description: description);
        }

        private void ProcessTypename()
        {
            if (CurrentTokenIsTypename)
            {
                Process();
            }
            else
            {
                throw new Exception($"Expected typename (int/char/bool/classname). Current token: {tokenizer.CurrentToken}.");
            }
        }

        private void ProcessOneOf(params Keyword[] keywords)
        {            
            if (keywords.Contains((Keyword)ct.Keyword))
            {
                Process();
            }
            else
            {
                throw new Exception($"Expected keyword to be one of: '{string.Join(",", keywords)}'. Current token: '{ct}'.");
            }
        }

        //private void ProcessSubroutineCall()
        //{
        //    var firstToken = ct;
        //    Advance();
            
        //    if(ct.TokenType == TokenType.Symbol && ct.Value == ".")
        //    {
        //        if(firstToken.TokenType == TokenType.Identifier)
        //        {
        //            WriteXml(firstToken); // class name or variable name
        //            ProcessSymbol(".");
        //            ProcessIdentifier("subroutine name");
        //            ProcessParameterList();
        //        }
        //        else
        //        {
        //            throw new Exception("In a subroutine call, only an identifier can be proceeded by a '.'");
        //        }
                
        //    }
        //    else if (ct.TokenType == TokenType.Symbol && ct.Value == "(")
        //    {
        //        if (firstToken.TokenType == TokenType.Identifier)
        //        {
        //            WriteXml(firstToken); // subroutine name
        //            ProcessParameterList();
        //        }
        //        else
        //        {
        //            throw new Exception("Expected an identifier proceeding the parameter list for a subroutine call");
        //        }
        //    }
        //    else
        //    {
        //        throw new Exception("not a subroutine call");
        //    }


        //}

        private void ProcessParameterList()
        {
            ProcessSymbol("(");

            if (ct.TokenType == TokenType.Symbol && ct.Value == ")")
            {
                ProcessSymbol(")");
            }
            else
            {
                CompileExpression();
                while(CurrentTokenIsComma)
                {
                    ProcessSymbol(",");
                    CompileExpression();
                }
                ProcessSymbol(")");
            }
        }
        

        private bool CurrentTokenIsClassVarDec => ct.Keyword switch
        {
            Keyword.Static => true,
            Keyword.Field => true,
            _ => false
        };

        private bool CurrentTokenIsSubroutineDec => ct.Keyword switch
        {
            Keyword.Constructor => true,
            Keyword.Function => true,
            Keyword.Method => true,
            _ => false
        };

        private bool CurrentTokenIsTypename => ct switch
        {
            Token t when t.Keyword == Keyword.Int => true,
            Token t when t.Keyword == Keyword.Char => true,
            Token t when t.Keyword == Keyword.Boolean => true,
            Token t when t.TokenType == TokenType.Identifier => true,
            _ => false
        };

        private bool CurrentTokenIsStatement => ct switch
        {
            Token t when t.Keyword == Keyword.Let => true,
            Token t when t.Keyword == Keyword.If => true,
            Token t when t.Keyword == Keyword.While => true,
            Token t when t.Keyword == Keyword.Do => true,
            Token t when t.Keyword == Keyword.Return => true,
            _ => false
        };

        private bool CurrentTokenIsKeywordConstant => ct.TokenType == TokenType.Keyword && ct.Keyword switch
        {
            Keyword.True => true,
            Keyword.False => true,
            Keyword.This => true,
            Keyword.Null => true,
            _ => false
        };

        private bool CurrentTokenIsUnaryOp => ct.TokenType == TokenType.Symbol && (ct.Value == "-" || ct.Value == "~");

        private bool CurrentTokenIsComma => ct.TokenType == TokenType.Symbol && ct.Value == ",";
        private bool CurrentTokenIsOpenParen => ct.TokenType == TokenType.Symbol && ct.Value == "(";

        private static readonly List<string> BinaryOperators = new List<string> { "+", "-", "*", "/", "&", "|", "<", ">", "=" };
        private bool CurrentTokenIsOp => ct.TokenType == TokenType.Symbol && BinaryOperators.Contains(ct.Value);
        

    }
}
