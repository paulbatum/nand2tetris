using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace HDLTools.Test
{
    public class OrTests
    {
        private class MyTestData : BinaryTestData
        {
            public MyTestData() : base("Or.cmp")
            { }
        }

        private readonly ITestOutputHelper testOutput;
        public OrTests(ITestOutputHelper output)
        {
            this.testOutput = output;
        }

        [Theory]
        [ClassData(typeof(MyTestData))]
        public void BasicOr(int a, int b, int outValue)
        {
            var library = new ChipLibrary();
            library.Register(HDLParser.ParseString(File.ReadAllText(@"hdl\Not.hdl")).Single());
            library.Register(HDLParser.ParseString(File.ReadAllText(@"hdl\And.hdl")).Single());

            var hdl = File.ReadAllText(@"hdl\Or.hdl");
            ChipDescription desc = HDLParser.ParseString(hdl).Single();

            Chip chip = new Chip(desc, library);

            var cycle = 0;
            var pinA = chip.Pins.Single(x => x.Name == "a");
            var pinB = chip.Pins.Single(x => x.Name == "b");
            var pinOut = chip.Pins.Single(x => x.Name == "out");
            pinA.Init(a);
            pinB.Init(b);

            chip.Simulate(cycle);

            testOutput.DumpChip(chip, cycle);

            Assert.Equal(outValue, pinOut.GetBit(cycle));
        }
    }
}
