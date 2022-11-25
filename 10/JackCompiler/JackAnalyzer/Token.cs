using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackAnalyzer
{
    public class Token
    {
        public Token(TokenType tokenType)
        {
            this.TokenType = tokenType;
        }

        public TokenType TokenType { get; set; }
    }

    public class KeywordToken : Token
    {
        public KeywordToken() : base(TokenType.Keyword)
        {

        }

        public Keyword Keyword { get; set; }
    }

    public class SymbolToken : Token
    {
        public SymbolToken() : base(TokenType.Symbol)
        {

        }

        public char Symbol { get; set; }
    }

    public class IdentifierToken : Token
    {
        public IdentifierToken() : base(TokenType.Identifier)
        {
        }

        public string Identifier { get; set; }
    }

    public enum TokenType
    {
        Keyword,
        Symbol,
        Identifier,
        Int_Const,
        String_Const
    }

    public enum Keyword
    {
        Class,
        Method,
        Function,
        Constructor,
        Int,
        Boolean,
        Char,
        Void,
        Var,
        Static,
        Field,
        Let,
        Do,
        If,
        Else,
        While,
        Return,
        True,
        False,
        Null,
        This
    }
}
