using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackAnalyzer
{
    public class JackTokenizer
    {
        private IEnumerator<Token> tokens;
        public JackTokenizer(StreamReader reader)
        {
            this.tokens = Tokenize(reader).GetEnumerator();
            HasMoreTokens = this.tokens.MoveNext();
        }

        public Token? CurrentToken { get; private set; } = null;
        public bool HasMoreTokens { get; private set; }

        public void Advance()
        {
            CurrentToken = this.tokens.Current;
            HasMoreTokens = tokens.MoveNext();
        }

        private IEnumerable<Token> Tokenize(StreamReader reader)
        {
            var keywords = typeof(Keyword)
                .GetEnumNames()
                .Select(x => x.ToLower());

            var symbols = new char[] { '{', '}', '(', ')', '[', ']', '.', ',', ';', '+', '-', '*', '/', '&', '|', '<', '>', '=', '~' };

            char readChar;
            while(!reader.EndOfStream)
            {
                readChar = (char) reader.Read();

                if (char.IsWhiteSpace(readChar))
                    continue;

                if(readChar == '/')
                {
                    if (reader.Peek() == '/')
                    {
                        reader.ReadLine();
                        continue;
                    }

                    if (reader.Peek() == '*')
                    {
                        reader.Read(); // consume the *
                        while (!reader.EndOfStream)
                        {
                            readChar = (char)reader.Read();
                            if (readChar == '*' && reader.Peek() == '/')
                            {
                                reader.Read(); // consume the /
                                break;
                            }
                        }
                        continue;
                    }
                }

                if(symbols.Contains(readChar))
                {
                    yield return new Token(TokenType.Symbol, readChar);
                    continue;
                }

                if(char.IsDigit(readChar))
                {
                    string integerConstant = readChar.ToString();
                    while(!reader.EndOfStream && char.IsDigit((char) reader.Peek()))
                    {
                        readChar = (char)reader.Read();
                        integerConstant += readChar;
                    }
                    yield return new Token(TokenType.IntegerConstant, integerConstant);
                    continue;
                }
                
                if(char.IsLetter(readChar) || readChar == '_')
                {
                    bool IsIdentifierChar(char c) => char.IsLetterOrDigit(c) || c == '_';
                    string keywordOrIdentifier = readChar.ToString();
                    while(!reader.EndOfStream && IsIdentifierChar((char)reader.Peek()))
                    {
                        readChar = (char)reader.Read();
                        keywordOrIdentifier += readChar;
                    }

                    if(keywords.Contains(keywordOrIdentifier))
                    {
                        yield return new Token(TokenType.Keyword, keywordOrIdentifier);
                        continue;
                    }

                    yield return new Token(TokenType.Identifier, keywordOrIdentifier);
                    continue;
                }

                if(readChar == '"')
                {
                    string stringLiteral = "";
                    while (!reader.EndOfStream && reader.Peek() != '"')
                    {
                        readChar = (char)reader.Read();
                        stringLiteral += readChar;
                    }
                    reader.Read(); // consume the end quote
                    yield return new Token(TokenType.StringConstant, stringLiteral);
                    continue;
                }

                // we shouldn't be here
                Debug.WriteLine("error - dumping remaining input");
                Debug.WriteLine(reader.ReadToEnd());
                throw new Exception($"Failed to tokenize");
            }
        }
    }

    public record Token(TokenType TokenType, object Value);

    public enum TokenType
    {
        Keyword,
        Symbol,
        Identifier,
        IntegerConstant,
        StringConstant
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
