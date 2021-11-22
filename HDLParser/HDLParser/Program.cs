using Parlot.Fluent;
using System.Text;
using static Parlot.Fluent.Parsers;

var input = File.ReadAllText(@"Not.hdl");


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
    .AndSkip(Literals.WhiteSpace())
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

var parser = chip
    .SkipAnd(Terms.Identifier())
    .AndSkip(Terms.Char('{'))
    .And(inputPins)
    .And(outputPins)
    .And(parts)
    .Then(x => new Chip(x.Item1.ToString(), x.Item2, x.Item3, x.Item4));

var output = parser.Parse(input);

//input = "Nand (a=in, b=in, out=out);";
//input = "a=in";
//var output = part.Parse(input);

Console.WriteLine(output);

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
