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
        private StreamWriter xmlOutput;
        private VMWriter vmWriter;
        private bool expectingLastToken = false;
        private SymbolTable classTable = new SymbolTable();
        private SymbolTable subroutineTable = new SymbolTable();
        private SymbolTable currentTable;
        private StringBuilder processedTokens = new StringBuilder();
        private string className;

        public CompilationEngine(StreamReader input, StreamWriter xmlOutput, VMWriter vmWriter)
        {
            tokenizer = new JackTokenizer(input);    
            this.xmlOutput = xmlOutput;
            this.vmWriter = vmWriter;
            currentTable = classTable;
    }

        private Token ct => tokenizer.CurrentToken ?? throw new Exception("Why is the current token null?");        
        
        public void CompileClass()
        {
            // There is no initial token.
            Advance();

            WriteStartElement("class");
            ProcessKeyword(Keyword.Class);
            className = ProcessClassNameDeclared();
            ProcessCharacterSymbol("{");
          
            while (CurrentTokenIsClassVarDec)
            {
                CompileClassVarDec();
            }

            while (CurrentTokenIsSubroutineDec)
            {
                CompileSubroutineDec();
            }

            expectingLastToken = true;
            ProcessCharacterSymbol("}");
            WriteEndElement("class");            
        }

        private void CompileClassVarDec()
        {
            WriteStartElement("classVarDec");
            var symbolKind = ProcessOneOf(Keyword.Static, Keyword.Field) switch
            {
                Keyword.Static => SymbolKind.Static,
                Keyword.Field => SymbolKind.Field,
                _ => throw new Exception("not possible"),
            };

            var symbolType = ProcessTypename();
            ProcessSymbolDeclared(symbolKind, symbolType);

            while(CurrentTokenIsComma)
            {
                ProcessCharacterSymbol(",");
                ProcessSymbolDeclared(symbolKind, symbolType);
            }

            ProcessCharacterSymbol(";");
            WriteEndElement("classVarDec");
        }

        private void CompileSubroutineDec()
        {
            subroutineTable = new SymbolTable();
            currentTable = subroutineTable;
            WriteStartElement("subroutineDec");

            ProcessOneOf(Keyword.Constructor, Keyword.Function, Keyword.Method);
            if (ct.TokenType == TokenType.Keyword && ct.Keyword == Keyword.Void)
            {
                Process();
            }
            else
            {
                ProcessTypename();
            }

            var subroutineName = ProcessSubroutineNameDeclared();
            ProcessCharacterSymbol("(");
            var parameterCount = CompileParameterList();
            ProcessCharacterSymbol(")");

            vmWriter.WriteFunction($"{className}.{subroutineName}", parameterCount);
            
            WriteStartElement("subroutineBody");
            ProcessCharacterSymbol("{");

            while(ct.Keyword == Keyword.Var)
            {
                WriteStartElement("varDec");
                Process(); // var keyword
                var symbolType = ProcessTypename();
                ProcessSymbolDeclared(SymbolKind.Var, symbolType);                
                while(CurrentTokenIsComma)
                {
                    Process();
                    ProcessSymbolDeclared(SymbolKind.Var, symbolType);
                }

                ProcessCharacterSymbol(";");
                WriteEndElement("varDec");
            }

            CompileStatements();                       

            ProcessCharacterSymbol("}");
            WriteEndElement("subroutineBody");

            WriteEndElement("subroutineDec");
            currentTable = classTable;
        }

        public void CompileStatements()
        {
            WriteStartElement("statements");
            
            while (CurrentTokenIsStatement)
            {
                switch(ct.Keyword)
                {
                    case Keyword.Let:
                        CompileLet();
                        break;
                    case Keyword.If:
                        CompileIf();
                        break;
                    case Keyword.While:
                        CompileWhile();
                        break;
                    case Keyword.Do:
                        CompileDo();
                        break;
                    case Keyword.Return:
                        CompileReturn();
                        break;
                    default:
                        throw new Exception("unreachable");
                }
            }

            WriteEndElement("statements");
        }

        private void CompileLet()
        {
            WriteStartElement("letStatement");
            Process(); // let keyword
            ProcessSymbolUsed(); 
            if (ct.TokenType == TokenType.Symbol && ct.Value == "[")
            {
                Process(); // the opening bracket
                CompileExpression();
                ProcessCharacterSymbol("]");
            }

            ProcessCharacterSymbol("=");
            CompileExpression();
            ProcessCharacterSymbol(";");
            WriteEndElement("letStatement");
        }

        private void CompileIf()
        {
            WriteStartElement("ifStatement");
            Process(); // if keyword
            ProcessCharacterSymbol("(");
            CompileExpression();
            ProcessCharacterSymbol(")");
            ProcessCharacterSymbol("{");
            CompileStatements();
            ProcessCharacterSymbol("}");

            if (ct.Keyword == Keyword.Else)
            {
                Process(); // else keyword
                ProcessCharacterSymbol("{");
                CompileStatements();
                ProcessCharacterSymbol("}");
            }
            WriteEndElement("ifStatement");
        }

        private void CompileWhile()
        {
            WriteStartElement("whileStatement");
            Process(); // while keyword
            ProcessCharacterSymbol("(");
            CompileExpression();
            ProcessCharacterSymbol(")");
            ProcessCharacterSymbol("{");
            CompileStatements();
            ProcessCharacterSymbol("}");
            WriteEndElement("whileStatement");
        }

        private void CompileDo()
        {
            WriteStartElement("doStatement");
            Process(); // do keyword                       
            CompileTerm(isDo: true);
            ProcessCharacterSymbol(";");
            WriteEndElement("doStatement");
        }

        private void CompileReturn()
        {
            WriteStartElement("returnStatement");
            Process(); // return keyword
            if (ct.TokenType == TokenType.Symbol && ct.Value == ";")
            {
                ProcessCharacterSymbol(";");
            }
            else
            {
                CompileExpression();
                ProcessCharacterSymbol(";");
            }

            vmWriter.WriteReturn();
            WriteEndElement("returnStatement");
        }

        public void CompileTerm(bool isDo = false)
        {
            if (!isDo)
            {
                WriteStartElement("term");
            }

            if (ct.TokenType == TokenType.IntegerConstant)
            {
                vmWriter.WritePush(Segment.Constant, int.Parse(ct.Value));
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
                ProcessCharacterSymbol("(");
                CompileExpression();
                ProcessCharacterSymbol(")");
            }
            else if (CurrentTokenIsUnaryOp)
            {
                var op = ct;
                Advance();
                CompileTerm();
                CompileUnary(op);
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
                            ProcessSymbolUsed(identifier, SymbolCategory.Subroutine.ToString());
                            ProcessCharacterSymbol("(");
                            CompileExpressionList();
                            ProcessCharacterSymbol(")");
                            break;
                        case "[":
                            ProcessSymbolUsed(identifier);
                            ProcessCharacterSymbol("[");
                            CompileExpression();
                            ProcessCharacterSymbol("]");
                            break;
                        case ".":
                            var i1 = ProcessSymbolUsed(identifier, SymbolCategory.Class.ToString());
                            ProcessCharacterSymbol(".");
                            var i2 = ProcessSymbolUsed(SymbolCategory.Subroutine.ToString());
                            ProcessCharacterSymbol("(");
                            var expressionCount = CompileExpressionList();
                            ProcessCharacterSymbol(")");
                            vmWriter.WriteCall($"{i1}.{i2}", expressionCount);
                            break;
                        default:
                            var i3 = ProcessSymbolUsed(identifier);
                            var symbol = currentTable.Lookup(i3);
                            if (symbol != null)
                            {
                                vmWriter.WritePush(symbol.Segment, symbol.Index);
                            }
                            break;
                    }
                }
            }
            else
            {
                throw new Exception($"Expected a term, got '{ct}'.");
            }
             
            if (isDo)
            {
                vmWriter.WritePop(Segment.Temp, 0);
            }
            else
            {
                WriteEndElement("term");
            }

        }

        private void CompileExpression()
        {
            WriteStartElement("expression");

            CompileTerm();
            while (CurrentTokenIsBinaryOp)
            {
                var op = ct;               
                Advance();
                CompileTerm();
                CompileBinaryOp(op);
            }

            WriteEndElement("expression");
        }

        private void CompileBinaryOp(Token op)
        {
            switch(op.Value)
            {
                case "+":
                    vmWriter.WriteArithmetic("add");
                    break;
                case "-":
                    vmWriter.WriteArithmetic("sub");
                    break;
                case "*":
                    vmWriter.WriteCall("Math.multiply", 2);
                    break;
                case "/":
                    vmWriter.WriteCall("Math.divide", 2);
                    break;
                case "<":
                    vmWriter.WriteArithmetic("lt");
                    break;
                case ">":
                    vmWriter.WriteArithmetic("gt");
                    break;
                case "&":
                    vmWriter.WriteArithmetic("and");
                    break;
                case "|":
                    vmWriter.WriteArithmetic("or");
                    break;
                case "=":
                    vmWriter.WriteArithmetic("eq");
                    break;
                default:
                    throw new Exception($"Unrecognized op: {op.Value}");
            }
        }

        private void CompileUnary(Token op)
        {
            switch(op.Value)
            {
                case "-":
                    vmWriter.WriteArithmetic("neg");
                    break;
                case "~":
                    vmWriter.WriteArithmetic("not");
                    break;
                default:
                    throw new Exception($"Unrecognized op: {op.Value}");
            }
        }

        private int CompileParameterList()
        {
            int parameterCount = 0;
            WriteStartElement("parameterList");
            while (CurrentTokenIsTypename)
            {
                var symbolType = ProcessTypename();
                ProcessSymbolDeclared(SymbolKind.Arg, symbolType);
                parameterCount += 1;
                if (CurrentTokenIsComma)
                {
                    Process(); // comma
                    continue; // I'm going to cheat and allow a trailing comma
                }

                break;
            }
            WriteEndElement("parameterList");

            return parameterCount;
        }

        private int CompileExpressionList()
        {
            int expressionCount = 0;
            WriteStartElement("expressionList");

            if (ct.TokenType != TokenType.Symbol || ct.Value != ")")
            { 
                CompileExpression();
                expressionCount += 1;
                while (CurrentTokenIsComma)
                {
                    ProcessCharacterSymbol(",");
                    CompileExpression();
                    expressionCount += 1;
                }                
            }

            WriteEndElement("expressionList");
            return expressionCount;
        }

        private void Advance()
        {
            if (tokenizer.HasMoreTokens)
            {
                var t = tokenizer.CurrentToken;
                if (t != null)
                {
                    if (";{}".Contains(t.Value))
                    {
                        processedTokens.AppendLine(t.Value);
                    }
                    else
                    {
                        processedTokens.Append(t.Value);
                        processedTokens.Append(" ");
                    }                    
                }
                
                tokenizer.Advance();
            }
            else
            {
                if (expectingLastToken == false)
                    throw new Exception("Unexpected end of token stream");
            }                
        }

        private void Dump() => System.Diagnostics.Debug.WriteLine(processedTokens.ToString());
        private void WriteStartElement(string element) => xmlOutput.WriteLine($"<{element}>");
        private void WriteEndElement(string element) => xmlOutput.WriteLine($"</{element}>");

        private void WriteXml(Token token)
        {
            string elementName = token.TokenType.ToString();
            elementName = elementName.Substring(0,1).ToLower() + elementName.Substring(1);

            string outVal = token.Value switch
            {
                string s when s == ">" => "&gt;",
                string s when s == "<" => "&lt;",
                string s when s == "&" => "&amp;",
                string s => s
            };

            WriteXmlElement(elementName, outVal);
        }

        private void WriteXmlElement(string elementName, string elementValue)
        {
            xmlOutput.WriteLine($"<{elementName}> {elementValue} </{elementName}>");
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

        private void ProcessCharacterSymbol(string expectedValue)
        {
            Process(TokenType.Symbol, expectedValue: expectedValue);
        }

        private string ProcessClassNameDeclared()
        {
            if (ct.TokenType != TokenType.Identifier)
                throw new Exception($"Expected identifier for class name. Current token: '{ct}'.");

            var className = ct.Value;

            WriteIdentifierXml(ct.Value, SymbolCategory.Class.ToString(), SymbolUsage.Declared);
            Advance();

            return className;
        }

        private string ProcessSubroutineNameDeclared()
        {
            if (ct.TokenType != TokenType.Identifier)
                throw new Exception($"Expected identifier for subroutine name. Current token: '{ct}'.");

            var subroutineName = ct.Value;

            WriteIdentifierXml(subroutineName, SymbolCategory.Subroutine.ToString(), SymbolUsage.Declared);
            Advance();

            return subroutineName;
        }

        private void ProcessSymbolDeclared(SymbolKind kind, string symbolType)
        {
            if (ct.TokenType != TokenType.Identifier)
                throw new Exception($"Expected identifier for {kind} name. Current token: '{ct}'.");

            currentTable.Define(ct.Value, symbolType, kind);
            var s = currentTable.Lookup(ct.Value)!;

            WriteIdentifierXml(ct.Value, kind.ToString(), SymbolUsage.Declared, s.Index);
            Advance();
        }

        private string ProcessSymbolUsed(string typeHint = "dont know")
        {
            var symbol = ProcessSymbolUsed(ct, typeHint);
            Advance();
            return symbol;
        }

        private string ProcessSymbolUsed(Token token, string typeHint = "dont know")
        {
            if (token.TokenType != TokenType.Identifier)
                throw new Exception($"Expected identifier. Token: '{token}'.");

            var s = subroutineTable.Lookup(token.Value) ?? classTable.Lookup(token.Value);

            if (s == null)
            {
                // this must be a subroutine name / class name
                WriteIdentifierXml(token.Value, typeHint, SymbolUsage.Used);
                return token.Value;
            }
            else
            {
                WriteIdentifierXml(s.Name, s.Kind.ToString(), SymbolUsage.Used, s.Index);
                return s.Name;
            }
            
        }

        private void WriteIdentifierXml(string name, string category, SymbolUsage usage, int? index = null)
        {
            WriteStartElement("identifier");

            WriteXmlElement("name", name);
            WriteXmlElement("category", category.ToLower());
            WriteXmlElement("usage", usage.ToString().ToLower());

            if(index != null)
                WriteXmlElement("index", index.ToString());

            WriteEndElement("identifier");            
        }        

        private string ProcessTypename()
        {
            var v = ct.Value;

            if (CurrentTokenIsBuiltInType)
            {
                Process(); //int/char/bool
            }
            else if (ct.TokenType == TokenType.Identifier)
            {
                ProcessSymbolUsed(SymbolCategory.Class.ToString());
            }
            else
            {
                throw new Exception($"Expected typename (int/char/bool/classname). Current token: {tokenizer.CurrentToken}.");
            }

            return v;
        }

        private Keyword ProcessOneOf(params Keyword[] keywords)
        {
            if(ct.TokenType != TokenType.Keyword)
                throw new Exception($"Expected keyword. Current token: '{ct}'.");

            var k = (Keyword)ct.Keyword;

            if (keywords.Contains(k))
            {                
                Process();
                return k;
            }
            else
            {
                throw new Exception($"Expected keyword to be one of: '{string.Join(",", keywords)}'. Current token: '{ct}'.");
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

        private bool CurrentTokenIsBuiltInType => ct.Keyword switch
        {
            Keyword.Int => true,
            Keyword.Char => true,
            Keyword.Boolean => true,
            _ => false
        };

        private bool CurrentTokenIsTypename => CurrentTokenIsBuiltInType || ct.TokenType == TokenType.Identifier;

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
        private bool CurrentTokenIsBinaryOp => ct.TokenType == TokenType.Symbol && BinaryOperators.Contains(ct.Value);
        
        private enum SymbolCategory
        {
            Field = SymbolKind.Field,
            Static = SymbolKind.Static,
            Var = SymbolKind.Var,
            Arg = SymbolKind.Arg,
            Class,
            Subroutine,
        }

        private enum SymbolUsage
        {
            Declared,
            Used,
        }
    }
}
