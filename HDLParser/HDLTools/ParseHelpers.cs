using Parlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDLTools
{
    public class ParseHelpers
    {
        public static string RemoveComments(string input)
        {
            var scanner = new Scanner(input);
            var builder = new StringBuilder();

            while (!scanner.Cursor.Eof)
            {
                scanner.ReadWhile(c => c != '/', out TokenResult result);

                if (result.Length > 0)
                {
                    builder.Append(result.GetText());
                }

                if (scanner.Cursor.Match("//"))
                {
                    while (!Character.IsNewLine(scanner.Cursor.Current))
                        scanner.Cursor.Advance();
                    scanner.SkipWhiteSpaceOrNewLine();
                }
                else if (scanner.Cursor.Match("/*"))
                {
                    while (!scanner.Cursor.Match("*/"))
                        scanner.Cursor.Advance();
                    scanner.Cursor.Advance();
                    scanner.Cursor.Advance();
                }
            }

            return builder.ToString().Trim();
        }
    }
}
