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

        public CompilationEngine(StreamReader input, StreamWriter output)
        {
            tokenizer = new JackTokenizer(input);    
            this.output = output;
        }



        public void CompileClass()
        {
            tokenizer.Advance();
            output.WriteLine("<class>");
            Process(Keyword.Class);
            Process(TokenType.Identifier);
            Process(TokenType.Symbol, "{");
          
            while (CurrentTokenIsClassVarDec)
            {
                CompileClassVarDec();
            }

            while (CurrentTokenIsSubroutineDec)
            {
                CompileSubroutineDec();
            }

            Process(TokenType.Symbol, "}");

            output.WriteLine("</class>");
        }

        private void CompileClassVarDec()
        {
            output.WriteLine("<classVarDec>");

            Process((Keyword) tokenizer.CurrentToken.Keyword); // static or field

            Token typeToken = tokenizer.CurrentToken switch
            {
                Token t when t.Keyword == Keyword.Int => t,
                Token t when t.Keyword == Keyword.Char => t,
                Token t when t.Keyword == Keyword.Boolean => t,
                Token t when t.TokenType == TokenType.Identifier => t,
                _ => throw new Exception($"Expected typename (int/char/bool/classname). Current token: {tokenizer.CurrentToken}.")
            };

            Process(typeToken);
            Process(TokenType.Identifier); // variable name
            while(tokenizer.CurrentToken.TokenType == TokenType.Symbol && tokenizer.CurrentToken.Value.ToString() == ",")
            {
                Process(TokenType.Symbol, ",");
                Process(TokenType.Identifier); // more variable names
            }

            Process(TokenType.Symbol, ";");
            output.WriteLine("</classVarDec>");
        }

        private void CompileSubroutineDec()
        {
            output.WriteLine("<subroutineDec>");

            output.WriteLine("</subroutineDec>");
            tokenizer.Advance();
        }

        private void Process(Token token)
        {
            WriteXml(tokenizer.CurrentToken);
            tokenizer.Advance();
        }

        private void Process(Keyword keyword)
        {
            var ct = tokenizer.CurrentToken;
            if(ct.Keyword == keyword)
            {
                WriteXml(ct);
            }
            else
            {
                throw new Exception($"Expected keyword: '{keyword}'. Current token: '{ct}'.");
            }

            tokenizer.Advance();
        }

        private void Process(TokenType tokenType)
        {
            var ct = tokenizer.CurrentToken;
            if(ct.TokenType == tokenType)
            {
                WriteXml(ct);
            }
            else
            {
                throw new Exception($"Expected token of type '{tokenType}'. Current token: '{ct}'.");
            }

            tokenizer.Advance();
        }

        private void Process(TokenType tokenType, string str)
        {
            var ct = tokenizer.CurrentToken;
            if (ct.TokenType == tokenType && ct.Value == str)
            {
                WriteXml(ct);
            }
            else
            {
                throw new Exception($"Expected token of type '{tokenType}' with value '{str}'. Current token: '{ct}'.");
                //throw new Exception($"Expected token of type '{tokenType}' with value '{str}' but current token is of type '{ct.TokenType}' with value '{ct.Value}'.");
            }

            tokenizer.Advance();
        }

        private void WriteXml(Token token)
        {
            string elementName = token.TokenType.ToString().ToLower();
            output.WriteLine($"<{elementName}> {token.Value} </{elementName}>");
        }

        private bool CurrentTokenIsClassVarDec => tokenizer.CurrentToken.Keyword switch
        {
            Keyword.Static => true,
            Keyword.Field => true,
            _ => false
        };

        private bool CurrentTokenIsSubroutineDec => tokenizer.CurrentToken.Keyword switch
        {
            Keyword.Constructor => true,
            Keyword.Function => true,
            Keyword.Method => true,
            _ => false
        };





    }
}
