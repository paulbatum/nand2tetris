using Parlot;
using Parlot.Fluent;
using System.Text;
using static Parlot.Fluent.Parsers;

var input = File.ReadAllText(@"Test.hdl");

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
    else if(Character.IsWhiteSpaceOrNewLine(scanner.Cursor.Current))
    {
        scanner.SkipWhiteSpaceOrNewLine();
    }
    else
    {
        break;
    }
}

Console.WriteLine("SKIPPING:");
Console.WriteLine(input.Substring(0, scanner.Cursor.Offset));
Console.WriteLine();

input = input.Substring(scanner.Cursor.Offset);

Console.WriteLine("PROCESSING:");
Console.WriteLine(input);

Console.WriteLine("OUTPUT:");
Console.WriteLine();

var chip = Terms.Text("CHIP");

var pinList = Separated(Terms.Char(','), Terms.Identifier())
    .AndSkip(Terms.Char(';'));

var inputPins = Terms.Text("IN")
    .SkipAnd(pinList)
    .Then(x => x.Select(x => new Pin(x.ToString())).ToList());

var outputPins = Terms.Text("OUT")
    .SkipAnd(pinList)
    .Then(x => x.Select(x => new Pin(x.ToString())).ToList());

var pinAssignment = Terms.Identifier()
    .AndSkip(Terms.Char('='))
    .And(Terms.Identifier())
    .Then(x => new PinAssignment(new Pin(x.Item1.ToString()), new Pin(x.Item2.ToString())));

var part = Terms.Identifier()    
    .And(Between(
        Terms.Char('('), 
        Separated(Terms.Char(','), pinAssignment), 
        Terms.Char(')')
        ))
    .AndSkip(Terms.Char(';'))
    .Then(x => new Part(x.Item1.ToString(), x.Item2));

var parts = Terms.Text("PARTS:")
    .SkipAnd(Literals.WhiteSpace(includeNewLines:true))
    .SkipAnd(OneOrMany(part));

var parser = OneOrMany(
    chip
    .SkipAnd(Terms.Identifier())
    .AndSkip(Terms.Char('{'))
    .And(inputPins)
    .And(outputPins)
    .And(parts)
    .AndSkip(Terms.Char('}'))
    .Then(x => new Chip(x.Item1.ToString(), x.Item2, x.Item3, x.Item4))
    );


List<Chip> output = parser.Parse(input);

foreach(var c in output)
{
    Console.WriteLine(c);
}

public class Chip
{
    public string Name { get; set; }
    public List<Pin> InputPins { get; }
    public List<Pin> OutputPins { get; }

    public List<Part> Parts { get; }

    public Chip(string name, List<Pin> inputPins, List<Pin> outputPins, List<Part> parts)
    {
        Name = name;
        InputPins = inputPins;
        OutputPins = outputPins;
        Parts = parts;
    }

    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.AppendLine($"CHIP {this.Name} {{");

        builder.Append("\tIN ");
        builder.AppendLine(string.Join(',', InputPins.Select(x => x.Name)) + ";");

        builder.Append("\tOUT ");
        builder.AppendLine(string.Join(',', OutputPins.Select(x => x.Name)) + ";");

        builder.AppendLine("\tPARTS:");
        foreach (var part in Parts)
        {
            builder.Append($"\t{part.Name} (");
            builder.Append(string.Join(',', part.PinAssignments.Select(x=>$"{x.Left.Name}={x.Right.Name}")));

            builder.AppendLine($")");
        }

        builder.AppendLine("}");
        return builder.ToString();
    }
}
public record Pin(string Name);
public record PinAssignment(Pin Left, Pin Right);
public record Part(string Name, List<PinAssignment> PinAssignments);
