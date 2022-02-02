using HDLTools;
using Parlot;
using Parlot.Fluent;
using System.Text;
using static Parlot.Fluent.Parsers;

var input = File.ReadAllText(@"tst\And.tst");

var output = TestScriptParser.ParseTestScriptString(input);

foreach (var c in output)
{
    Console.WriteLine(c);
}

