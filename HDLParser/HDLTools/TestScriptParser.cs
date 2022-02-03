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
    public class TestScriptParser
    {
        public static List<TestScriptCommand> ParseTestScriptString(string script)
        {
            script = ParseHelpers.RemoveComments(script);

            var terminator = Terms.Char(',')
                .Or(Terms.Char(';'))
                .Or(Terms.Char('!'));

            var load = Terms.Text("load")
                .SkipAnd(Terms.Identifier()).And(Terms.Text(".hdl"))
                .AndSkip(terminator)
                .Then(x => (TestScriptCommand) new LoadCommand(x.Item1 + x.Item2));

            var outputFile = Terms.Text("output-file")
                .SkipAnd(Terms.Identifier()).And(Terms.Text(".out"))
                .AndSkip(terminator)
                .Then(x => (TestScriptCommand) new OutputFileCommand(x.Item1 + x.Item2));

            var compareTo = Terms.Text("compare-to")
                .SkipAnd(Terms.Identifier()).And(Terms.Text(".cmp"))
                .AndSkip(terminator)
                .Then(x => (TestScriptCommand) new CompareToCommand(x.Item1 + x.Item2));

            var stringOutputFormat = Terms.Char('S')
                .Then(x => ValueFormat.String);

            var decimalOutputFormat = Terms.Char('D')
                .Then(x => ValueFormat.Decimal);

            var binaryOutputFormat = Terms.Char('B')
                .Then(x => ValueFormat.Binary);

            var hexOutputFormat = Terms.Char('X')
                .Then(x => ValueFormat.Hex);

            var outputFormat = stringOutputFormat
                .Or(decimalOutputFormat)
                .Or(binaryOutputFormat)
                .Or(hexOutputFormat);

            var outputSpecDefault = Terms.Identifier()
                .Then(x => new OutputSpec(x.ToString(), ValueFormat.Binary, 1, 1, 1));

            var outputSpec = Terms.Identifier()
                .AndSkip(Terms.Char('%'))
                .And(outputFormat)
                .And(Terms.Integer())
                .AndSkip(Terms.Char('.'))
                .And(Terms.Integer())
                .AndSkip(Terms.Char('.'))
                .And(Terms.Integer())
                .Then(x => new OutputSpec(x.Item1.ToString(), x.Item2, (int) x.Item3, (int) x.Item4, (int) x.Item5));

            var outputList = Terms.Text("output-list")
                .SkipAnd(OneOrMany(outputSpec.Or(outputSpecDefault)))
                .AndSkip(terminator)
                .Then(x => (TestScriptCommand)new OutputListCommand(x));

            var decimalVariableValue = Terms.Char('D')
                .SkipAnd(Terms.Integer())
                .Then(x => new VariableValue((int)x));

            var binaryVariableValue = Terms.Char('B')
                .SkipAnd(Terms.Pattern(c => c == '0' || c == '1', minSize: 1, maxSize: 16))
                .Then(x => new VariableValue(Convert.ToInt32(x.ToString(), 2)));

            var hexVariableValue = Terms.Char('X')
                .SkipAnd(Terms.Pattern(c => "0123456789ABCDEF".Contains(c), minSize: 1, maxSize: 4))
                .Then(x => new VariableValue(Convert.ToInt32(x.ToString(), 16)));

            var defaultVariableValue = Terms.Integer()
                .Then(x => new VariableValue((int)x));

            var variableValue = hexVariableValue
                .Or(binaryVariableValue)
                .Or(decimalVariableValue)
                .Or(defaultVariableValue);

            var setVariable = Terms.Text("set")
                .SkipAnd(Terms.Identifier())
                .And(variableValue)
                .AndSkip(terminator)
                .Then(x => (TestScriptCommand) new SetVariableCommand(x.Item1.ToString(), x.Item2));

            var eval = Terms.Text("eval")
                .AndSkip(terminator)
                .Then(x => (TestScriptCommand)new EvalCommand());

            var output = Terms.Text("output")
                .AndSkip(terminator)
                .Then(x => (TestScriptCommand)new OutputCommand());


            var parser = OneOrMany(
                load
                    .Or(outputFile)
                    .Or(compareTo)
                    .Or(outputList)
                    .Or(setVariable)
                    .Or(eval)
                    .Or(output)
            );

            return parser.Parse(script).ToList();
        }
    }

    public record TestScriptCommand
    {        

    }

    public record LoadCommand(string Filename) : TestScriptCommand;
    public record OutputFileCommand(string Filename) : TestScriptCommand;
    public record CompareToCommand(string Filename) : TestScriptCommand;
    public record OutputListCommand(List<OutputSpec> OutputSpecs) : TestScriptCommand;
    public record SetVariableCommand(string VariableName, VariableValue VariableValue) : TestScriptCommand;
    public record EvalCommand() : TestScriptCommand;
    public record OutputCommand() : TestScriptCommand;

    public enum ValueFormat
    {
        Binary,
        Decimal,
        String,
        Hex
    }
    public record OutputSpec(string VariableName, ValueFormat Format, int PadLeft, int Length, int PadRight);
    public record VariableValue(int Value);
}
