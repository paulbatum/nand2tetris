using HDLTools.TestScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace HDLTools.Test
{
    public class TestScriptParserTests
    {
        private readonly ITestOutputHelper testOutput;
        public TestScriptParserTests(ITestOutputHelper output)
        {
            this.testOutput = output;
        }

        private void AssertIsOutputSpec(OutputSpec spec, string variableName, ValueFormat format, int padLeft, int length, int padRight)
        {            
            Assert.Equal(variableName, spec.VariableName);
            Assert.Equal(format, spec.Format);
            Assert.Equal(padLeft, spec.PadLeft);
            Assert.Equal(length, spec.Length);
            Assert.Equal(padRight, spec.PadRight);
        }

        private void AssertIsSetCommand(TestScriptCommand command, string variableName, int value)
        {
            var setCommand = (SetVariableCommand)command;            
            Assert.Equal(variableName, setCommand.VariableName);
            Assert.Equal(value, setCommand.VariableValue.Value);

        }

        [Fact]
        public async Task ParsesAndScript()
        {
            var script = await File.ReadAllTextAsync("tst/And.tst");
            var commands = TestScriptParser.ParseString(script);

            Assert.Equal(20, commands.Count);

            var load = (LoadCommand)commands[0];
            Assert.Equal("And.hdl", load.Filename);

            var outputFile = (OutputFileCommand)commands[1];
            Assert.Equal("And.out", outputFile.Filename);

            var compareTo = (CompareToCommand)commands[2];
            Assert.Equal("And.cmp", compareTo.Filename);

            var outputList = (OutputListCommand)commands[3];
            Assert.Equal(3, outputList.OutputSpecs.Count);
            AssertIsOutputSpec(outputList.OutputSpecs[0], "a", ValueFormat.Binary, 3, 1, 3);
            AssertIsOutputSpec(outputList.OutputSpecs[1], "b", ValueFormat.Binary, 3, 1, 3);
            AssertIsOutputSpec(outputList.OutputSpecs[2], "out", ValueFormat.Binary, 3, 1, 3);

            AssertIsSetCommand(commands[4], "a", 0);
            AssertIsSetCommand(commands[5], "b", 0);
            Assert.IsType<EvalCommand>(commands[6]);
            Assert.IsType<OutputCommand>(commands[7]);

            AssertIsSetCommand(commands[8], "a", 0);
            AssertIsSetCommand(commands[9], "b", 1);
            Assert.IsType<EvalCommand>(commands[10]);
            Assert.IsType<OutputCommand>(commands[11]);

            AssertIsSetCommand(commands[12], "a", 1);
            AssertIsSetCommand(commands[13], "b", 0);
            Assert.IsType<EvalCommand>(commands[14]);
            Assert.IsType<OutputCommand>(commands[15]);

            AssertIsSetCommand(commands[16], "a", 1);
            AssertIsSetCommand(commands[17], "b", 1);
            Assert.IsType<EvalCommand>(commands[18]);
            Assert.IsType<OutputCommand>(commands[19]);
        }

        [Fact]
        public void ParsesBinaryValues()
        {
            var script = @"set x %B0000000000000000,";
            var commands = TestScriptParser.ParseString(script);

            Assert.Single(commands);

            AssertIsSetCommand(commands[0], "x", 0);
        }

        [Fact]
        public async Task ParsesAluScript()
        {
            var script = await File.ReadAllTextAsync("tst/Alu.tst");
            var commands = TestScriptParser.ParseString(script);

            Assert.Equal(296, commands.Count);
        }

        [Fact]
        public void ThrowsOnParseFailure()
        {
            var script = "load And.hdl," + Environment.NewLine + "asdf";
            Assert.Throws<InvalidTestScriptException>(() => TestScriptParser.ParseString(script));
        }

        [Fact]
        public async Task ParsesBitScript()
        {
            var script = await File.ReadAllTextAsync("tst/Bit.tst");
            var commands = TestScriptParser.ParseString(script);

            Assert.Equal(646, commands.Count);
        }

    }
}
