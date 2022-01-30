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
    public class AndTests
    {
        private class AndTestData : BinaryTestData
        {
            public AndTestData() : base("And.cmp")
            { }
        }

        private readonly ITestOutputHelper output;
        public AndTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory]
        [ClassData(typeof(AndTestData))]
        public void BasicAnd(int a, int b, int outValue)
        {
            var library = new ChipLibrary();
            library.Register(HDLParser.ParseString(File.ReadAllText("Not.hdl")).Single());

            var hdl = File.ReadAllText("And.hdl");
            ChipDescription desc = HDLParser.ParseString(hdl).Single();

            Chip chip = new Chip(desc, library);

            var cycle = 0;
            var pinA = chip.Pins.Single(x => x.Name == "a");
            var pinB = chip.Pins.Single(x => x.Name == "b");
            var pinOut = chip.Pins.Single(x => x.Name == "out");
            pinA.Init(a);
            pinB.Init(b);

            chip.Simulate(cycle);

            output.DumpChip(chip, cycle);

            Assert.Equal(outValue, pinOut.GetBit(cycle));
        }
    }
}
