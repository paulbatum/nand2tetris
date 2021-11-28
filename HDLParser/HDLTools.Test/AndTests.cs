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
        private readonly ITestOutputHelper output;
        public AndTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(1, 1, 1)]
        [InlineData(0, 1, 0)]
        [InlineData(1, 0, 0)]
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
            pinA.Values[cycle] = a;
            pinB.Values[cycle] = b;

            chip.Simulate(cycle);

            output.DumpChip(chip, cycle);

            Assert.Equal(outValue, pinOut.GetValue(cycle));
        }
    }
}
