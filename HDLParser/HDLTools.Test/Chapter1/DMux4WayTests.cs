using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace HDLTools.Test.Chapter1
{
    public class DMux4WayTests
    {
        private class DMux4WayTestData : BinaryTestData
        {
            public DMux4WayTestData() : base("DMux4Way.cmp")
            { }
        }

        private readonly ITestOutputHelper testOutput;
        public DMux4WayTests(ITestOutputHelper output)
        {
            this.testOutput = output;
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

            var pinIn = chip.Pins.Single(x => x.Name == "in");
            var pinSel = chip.Pins.Single(x => x.Name == "sel");
            var pinA = chip.Pins.Single(x => x.Name == "a");
            var pinB = chip.Pins.Single(x => x.Name == "b");
            var pinC = chip.Pins.Single(x => x.Name == "c");
            var pinD = chip.Pins.Single(x => x.Name == "d");

            pinIn.Init(@in);
            pinSel.Init(sel);

            chip.Evaluate();

            testOutput.WriteLine($"OUTPUT a:{pinA.GetBit()}, b: {pinB.GetBit()}, c:{pinC.GetBit()}, d:{pinD.GetBit()}");
            testOutput.DumpChip(chip);

            Assert.Equal(a, pinA.GetBit());
            Assert.Equal(b, pinB.GetBit());
            Assert.Equal(c, pinC.GetBit());
            Assert.Equal(d, pinD.GetBit());
        }
    }
}
