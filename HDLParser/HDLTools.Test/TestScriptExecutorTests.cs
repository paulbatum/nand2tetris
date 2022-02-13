using HDLTools.TestScripts;
using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace HDLTools.Test
{
    public class TestScriptExecutorTests    
    {
        private readonly ITestOutputHelper testOutput;
        public TestScriptExecutorTests(ITestOutputHelper output)
        {
            this.testOutput = output;
        }

        [Fact]
        public void ExecutesSetCommand()
        {
            var fs = new MockFileSystem();

            var library = new ChipLibrary();
            library.LoadAll("hdl");

            var loadCommand = new LoadCommand("Mux16.hdl");
            var setCommand = new SetVariableCommand("a", new VariableValue(16));

            var executor = new TestScriptExecutor(fs, library, new List<TestScriptCommand> { loadCommand, setCommand });

            while (executor.HasMoreLines)
                executor.Step();            

            var pin = executor.Chip!.Pins.Single(x => x.Name == "a");
            Assert.Equal(Conversions.ConvertDecimalIntToIntArray(setCommand.VariableValue.Value, pin.Width), pin.GetValue());
        }

        [Fact]
        public void InputValuesArePreserved()
        {
            var fs = new MockFileSystem();
            var library = new ChipLibrary();
            library.LoadAll("hdl");

            var commands = new List<TestScriptCommand>
            {
                new LoadCommand("ALU.hdl"),
                new OutputFileCommand("ALU.out"),
                new OutputListCommand( new List<OutputSpec> { new OutputSpec("x", ValueFormat.Binary, 1,16,1) }),
                new SetVariableCommand("x", new VariableValue(23)),
                new EvalCommand(),
                new OutputCommand(),
            };

            var executor = new TestScriptExecutor(fs, library, commands);

            while (executor.HasMoreLines)
                executor.Step();

            var pin = executor.Chip!.Pins.Single(x => x.Name == "x");
            Assert.Equal(Conversions.ConvertDecimalIntToBinaryString(23, pin.Width), Conversions.ConvertIntArrayToBinaryString(pin.GetValue()));
        }

        [Fact]
        public void OutputIsPadded()
        {
            var fs = new MockFileSystem();
            var library = new ChipLibrary();
            library.Register(HDLParser.ParseString(File.ReadAllText(@"hdl\Not.hdl")).Single());
            library.Register(HDLParser.ParseString(File.ReadAllText(@"hdl\Not16.hdl")).Single());

            var commands = new List<TestScriptCommand>
            {
                new LoadCommand("Not16.hdl"),
                new OutputFileCommand("Not16.out"),
                new OutputListCommand( new List<OutputSpec> { new OutputSpec("in", ValueFormat.Binary, 1,16,1) }),
                new EvalCommand(),
                new OutputCommand(),
            };

            var executor = new TestScriptExecutor(fs, library, commands);

            while (executor.HasMoreLines)
                executor.Step();

            var outputFile = fs.GetFile("Not16.out");

            var expected =
@"|        in        |
| 0000000000000000 |
";

            Assert.Equal(expected, outputFile.TextContents);
        }

        [Fact]
        public void HeaderSmallerIsPaddedExtraOnRight()
        {
            var spec = new OutputSpec("x", ValueFormat.Binary, 1, 16, 1);
            var header = '|' + OutputPadding.PadHeader(spec) + '|';
            Assert.Equal("|        x         |", header);
        }

        [Fact]
        public void HeaderBiggerIsPaddedExtraOnRight()
        {
            var spec = new OutputSpec("nx", ValueFormat.Binary, 1, 1, 1);
            var header = '|' + OutputPadding.PadHeader(spec) + '|';
            Assert.Equal("|nx |", header);
        }

        [Fact]
        public void AluOutMatchesAluCompare()
        {
            string compare = File.ReadAllText(@"cmp\ALU.cmp");

            var fs = new MockFileSystem();
            fs.AddFile(@"cmp\ALU.cmp", new MockFileData(compare));

            var library = new ChipLibrary();
            library.LoadAll("hdl");

            string script = File.ReadAllText(@"tst\ALU.tst");
            var commands = TestScriptParser.ParseString(script);
            var executor = new TestScriptExecutor(fs, library, commands);

            while (executor.HasMoreLines)
                executor.Step();

            Assert.Equal(compare, fs.GetFile("ALU.out").TextContents);
        }

        [Fact]
        public void ComparesUsingCompareFile()
        {
            var fs = new MockFileSystem();
            var library = new ChipLibrary();            

            var hdl =
                @"CHIP And {
                    IN a, b;
                    OUT out;

                    PARTS:
                    Nand(a=a, b=a, out=nandab); // should be b=b, this is a bug
                    Not(in=nandab, out=out);
                }";


            library.Register(HDLParser.ParseString(File.ReadAllText(@"hdl\Not.hdl")).Single());
            library.Register(HDLParser.ParseString(hdl).Single());

            string compare = File.ReadAllText(@"cmp\And.cmp");
            fs.AddFile(@"cmp\And.cmp", new MockFileData(compare));
            fs.AddFile(@"hdl\And.hdl", new MockFileData(hdl));

            string script = File.ReadAllText(@"tst\And.tst");
            var commands = TestScriptParser.ParseString(script);
            var executor = new TestScriptExecutor(fs, library, commands);

            while (executor.HasMoreLines)
                executor.Step();

            Assert.Single(executor.ComparisonFailures);
            Assert.Equal("|   1   |   0   |   0   |", executor.ComparisonFailures[0].CompareLine);
            Assert.Equal("|   1   |   0   |   1   |", executor.ComparisonFailures[0].OutputLine);
        }
    }
}
