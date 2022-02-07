using HDLTools;
using HDLTools.TestScripts;
using Parlot;
using Parlot.Fluent;
using System.Text;
using static Parlot.Fluent.Parsers;

var library = new ChipLibrary();

var hdlfiles = Directory.GetFiles("hdl", "*.hdl");
foreach (var hdlfile in hdlfiles)
{
    var content = File.ReadAllText(hdlfile);
    foreach (var chipDescription in HDLParser.ParseString(content))
    {
        library.Register(chipDescription);
    }
}

var script = File.ReadAllText(@"tst\Alu.tst");
var commands = TestScriptParser.ParseString(script);

foreach(var c in commands)
{
    Console.WriteLine(c);
}

var executor = new TestScriptExecutor(library, commands);

while (executor.HasMoreLines)
{
    executor.Step();
}
