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


        public static List<ChipDescription> ParseString(string input)
        {
            input = ParseHelpers.RemoveComments(input);

            var chip = Terms.Text("CHIP");

            var singleIndexer = Between(Terms.Char('['), Terms.Integer(), Terms.Char(']'))
                .Then(x => new Range { Start = (int)x, End = (int)x });
            var range = Terms.Integer()
                .AndSkip(Terms.Text(".."))
                .And(Terms.Integer());
            var rangeIndexer = Between(Terms.Char('['), range, Terms.Char(']'))
                .Then(x => new Range { Start = (int)x.Item1, End = (int)x.Item2 });
            var indexer = rangeIndexer.Or(singleIndexer);

            var unindexedPin = Terms.Identifier()
                .Then(x => new PinReference(Name: x.ToString(), IsIndexed: false, 0, 0));

            var indexedPin = Terms.Identifier()
                .And(indexer)
                .Then(x => new PinReference(Name: x.Item1.ToString(), IsIndexed: true, x.Item2.Start, x.Item2.End));

            var pin = indexedPin.Or(unindexedPin);

            var pinList = Separated(Terms.Char(','), pin)
                .AndSkip(Terms.Char(';'));

            var inputPins = Terms.Text("IN")
                .SkipAnd(pinList)
                .Then(x => x.Select(pinref => new PinDescription(Name: pinref.Name, Width: pinref.IsIndexed ? pinref.StartIndex : 1)).ToList());

            var outputPins = Terms.Text("OUT")
                .SkipAnd(pinList)
                .Then(x => x.Select(pinref => new PinDescription(Name: pinref.Name, Width: pinref.IsIndexed ? pinref.StartIndex : 1)).ToList());

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

        private record struct Range(int Start, int End);
    }
}
