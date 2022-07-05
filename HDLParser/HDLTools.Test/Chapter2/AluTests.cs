using HDLTools.TestScripts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace HDLTools.Test.Chapter2
{
    public class AluTests
    {
        private class AluNoStatData : BinaryTestData
        {
            public AluNoStatData() : base("ALU-nostat.cmp")
            { }
        }

        private class AluTestData : BinaryTestData
        {
            public AluTestData() : base("ALU.cmp")
            { }
        }

        private readonly ITestOutputHelper testOutput;
        public AluTests(ITestOutputHelper output)
        {
            this.testOutput = output;
        }

        [Theory]
        [ClassData(typeof(AluNoStatData))]
        public void AluNoStatCompareFile(int[] x, int[] y, int zx, int nx, int zy, int ny, int f, int no, int[] outValue)
        {
            var library = new ChipLibrary();
            library.LoadAll("hdl");

            var hdl = File.ReadAllText(@"hdl\ALU.hdl");
            ChipDescription desc = HDLParser.ParseString(hdl).Single();
            Chip chip = new Chip(desc, library);

            var singleInputPins = new[] { "zx", "nx", "zy", "ny", "f", "no" }    
                .Select(x => chip.Pins.Single(p => p.Name == x.ToString()))
                .ToArray();

            var pinX = chip.Pins.Single(x => x.Name == "x");
            var pinY = chip.Pins.Single(x => x.Name == "y");
            var pinOut = chip.Pins.Single(x => x.Name == "out");

            var singleInputValues = new[] { zx, nx, zy, ny, f, no };

            for (int index = 0; index < singleInputValues.Length; index++)
            {
                singleInputPins[index].Init(singleInputValues[index]);
            }

            pinX.Init(x);
            pinY.Init(y);

            chip.Evaluate();

            Assert.Equal(outValue, pinOut.GetIntArrayValue());
        }

        [Theory]
        [ClassData(typeof(AluTestData))]
        public void AluCompareFile(int[] x, int[] y, int zx, int nx, int zy, int ny, int f, int no, int[] outValue, int zr, int ng)
        {
            var library = new ChipLibrary();
            library.LoadAll("hdl");

            var hdl = File.ReadAllText(@"hdl\ALU.hdl");
            ChipDescription desc = HDLParser.ParseString(hdl).Single();
            Chip chip = new Chip(desc, library);

            var singleInputPins = new[] { "zx", "nx", "zy", "ny", "f", "no" }
                .Select(x => chip.Pins.Single(p => p.Name == x.ToString()))
                .ToArray();

            var pinX = chip.Pins.Single(x => x.Name == "x");
            var pinY = chip.Pins.Single(x => x.Name == "y");
            var pinOut = chip.Pins.Single(x => x.Name == "out");
            var pinZr = chip.Pins.Single(x => x.Name == "zr");
            var pinNg = chip.Pins.Single(x => x.Name == "ng");

            var singleInputValues = new[] { zx, nx, zy, ny, f, no };

            for (int index = 0; index < singleInputValues.Length; index++)
            {
                singleInputPins[index].Init(singleInputValues[index]);
            }

            pinX.Init(x);
            pinY.Init(y);

            chip.Evaluate();

            Assert.Equal(outValue, pinOut.GetIntArrayValue());
            Assert.Equal(zr, pinZr.GetBit());
            Assert.Equal(ng, pinNg.GetBit());
        }

        [Fact]
        public void PassesTestScript()
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

            Assert.Empty(executor.ComparisonFailures);
        }
    }
}
