using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parlot;
using Parlot.Fluent;
using static Parlot.Fluent.Parsers;


namespace HDLTools
{
    public class HDLParser
    {
        public static string SkipToChip(string input)
        {
            var scanner = new Scanner(input);

            while (true)
            {
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
                else if (Character.IsWhiteSpaceOrNewLine(scanner.Cursor.Current))
                {
                    scanner.SkipWhiteSpaceOrNewLine();
                }
                else
                {
                    break;
                }
            }

            //Console.WriteLine("SKIPPING:");
            //Console.WriteLine(input.Substring(0, scanner.Cursor.Offset));
            //Console.WriteLine();

            return input.Substring(scanner.Cursor.Offset);
        }

        public static List<ChipDescription> ParseString(string input)
        {
            input = SkipToChip(input);

            var chip = Terms.Text("CHIP");

            var pinList = Separated(Terms.Char(','), Terms.Identifier())
                .AndSkip(Terms.Char(';'));

            var inputPins = Terms.Text("IN")
                .SkipAnd(pinList)
                .Then(x => x.Select(x => new PinDescription(x.ToString())).ToList());

            var outputPins = Terms.Text("OUT")
                .SkipAnd(pinList)
                .Then(x => x.Select(x => new PinDescription(x.ToString())).ToList());

            var pinAssignment = Terms.Identifier()
                .AndSkip(Terms.Char('='))
                .And(Terms.Identifier())
                .Then(x => new PinAssignmentDescription(new PinDescription(x.Item1.ToString()), new PinDescription(x.Item2.ToString())));

            var part = Terms.Identifier()
                .And(Between(
                    Terms.Char('('),
                    Separated(Terms.Char(','), pinAssignment),
                    Terms.Char(')')
                    ))
                .AndSkip(Terms.Char(';'))
                .Then(x => new PartDescription(x.Item1.ToString(), x.Item2));

            var parts = Terms.Text("PARTS:")
                .SkipAnd(Literals.WhiteSpace(includeNewLines: true))
                .SkipAnd(OneOrMany(part));

            var parser = OneOrMany(
                chip
                .SkipAnd(Terms.Identifier())
                .AndSkip(Terms.Char('{'))
                .And(inputPins)
                .And(outputPins)
                .And(parts)
                .AndSkip(Terms.Char('}'))
                .Then(x => new ChipDescription(x.Item1.ToString(), x.Item2, x.Item3, x.Item4))
                );


            List<ChipDescription> output = parser.Parse(input);
            return output;
        }
    }
}
