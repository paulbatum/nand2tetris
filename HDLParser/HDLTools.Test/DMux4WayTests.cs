using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace HDLTools.Test
{
    public class DMux4WayTests
    {
        private class DMux4WayTestData : BinaryTestData
        {
            public DMux4WayTestData() : base("DMux4Way.cmp")
            { }
        }

        private readonly ITestOutputHelper output;
        public DMux4WayTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory]
        [ClassData(typeof(DMux4WayTestData))]
        public void BasicDMux4Way(int @in, int[] sel, int a, int b, int c, int d)
        {
            var library = new ChipLibrary();
            library.Register(HDLParser.ParseString(File.ReadAllText(@"hdl\And.hdl")).Single());
            library.Register(HDLParser.ParseString(File.ReadAllText(@"hdl\Not.hdl")).Single());
            library.Register(HDLParser.ParseString(File.ReadAllText(@"hdl\DMux.hdl")).Single());

            var hdl = File.ReadAllText(@"hdl\DMux4Way.hdl");
            ChipDescription desc = HDLParser.ParseString(hdl).Single();

            Chip chip = new Chip(desc, library);

            var cycle = 0;
            var pinIn = chip.Pins.Single(x => x.Name == "in");
            var pinSel = chip.Pins.Single(x => x.Name == "sel");
            var pinA = chip.Pins.Single(x => x.Name == "a");
            var pinB = chip.Pins.Single(x => x.Name == "b");
            var pinC = chip.Pins.Single(x => x.Name == "c");
            var pinD = chip.Pins.Single(x => x.Name == "d");

            pinIn.Init(@in);
            pinSel.Init(sel);

            chip.Simulate(cycle);


            output.WriteLine($"OUTPUT a:{pinA.GetBit(cycle)}, b: {pinB.GetBit(cycle)}, c:{pinC.GetBit(cycle)}, d:{pinD.GetBit(cycle)}");
            output.DumpChip(chip);
            

            Assert.Equal(a, pinA.GetBit(cycle));
            Assert.Equal(b, pinB.GetBit(cycle));
            Assert.Equal(c, pinC.GetBit(cycle));
            Assert.Equal(d, pinD.GetBit(cycle));
        }
    }
}
