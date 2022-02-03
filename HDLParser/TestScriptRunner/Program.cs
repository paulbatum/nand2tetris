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

var executor = new TestScriptExecutor(library, File.ReadAllText(@"tst\And.tst"));

while (executor.HasMoreLines)
{
    await executor.Step();
}
