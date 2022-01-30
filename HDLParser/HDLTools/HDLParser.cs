﻿using System;
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

        public static List<ChipDescription> ParseString(string input)
        {
            input = RemoveComments(input);

            var chip = Terms.Text("CHIP");


            var indexer = Between(Terms.Char('['), Terms.Integer(), Terms.Char(']'));

            var unindexedPin = Terms.Identifier()
                .Then(x => new PinReference(Name: x.ToString(), IsIndexed: false, -1));

            var indexedPin = Terms.Identifier()
                .And(indexer)
                .Then(x => new PinReference(Name: x.Item1.ToString(), IsIndexed: true, (int)x.Item2));

            var pin = indexedPin.Or(unindexedPin);

            var pinList = Separated(Terms.Char(','), pin)
                .AndSkip(Terms.Char(';'));

            var inputPins = Terms.Text("IN")
                .SkipAnd(pinList)
                .Then(x => x.Select(pinref => new PinDescription(Name: pinref.Name, Width: pinref.IsIndexed ? pinref.Index : 1)).ToList());

            var outputPins = Terms.Text("OUT")
                .SkipAnd(pinList)
                .Then(x => x.Select(pinref => new PinDescription(Name: pinref.Name, Width: pinref.IsIndexed ? pinref.Index : 1)).ToList());

            var pinAssignment = pin
                .AndSkip(Terms.Char('='))
                .And(pin)
                .Then(x => new PinAssignmentDescription(Left: x.Item1, Right: x.Item2));

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
