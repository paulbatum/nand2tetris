using HDLTools;
using Parlot;
using Parlot.Fluent;
using System.Text;
using static Parlot.Fluent.Parsers;

var input = File.ReadAllText(@"Test.hdl");

List<ChipDescription> output = HDLParser.ParseString(input);

foreach (var c in output)
{
    Console.WriteLine(c);
}

