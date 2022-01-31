using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace HDLTools.Test.Chapter1
{
    public class DMuxTests
    {
        private class DMuxTestData : BinaryTestData
        {
            public DMuxTestData() : base("DMux.cmp")
            { }
        }

        [Theory]
        [ClassData(typeof(DMuxTestData))]
        public void BasicDMux(int @in, int sel, int a, int b)
        {
            var library = new ChipLibrary();
            library.Register(HDLParser.ParseString(File.ReadAllText(@"hdl\And.hdl")).Single());
            library.Register(HDLParser.ParseString(File.ReadAllText(@"hdl\Not.hdl")).Single());

            var hdl = File.ReadAllText(@"hdl\DMux.hdl");
            ChipDescription desc = HDLParser.ParseString(hdl).Single();

            Chip chip = new Chip(desc, library);

            var cycle = 0;
            var pinIn = chip.Pins.Single(x => x.Name == "in");
            var pinSel = chip.Pins.Single(x => x.Name == "sel");
            var pinA = chip.Pins.Single(x => x.Name == "a");
            var pinB = chip.Pins.Single(x => x.Name == "b");

            pinIn.Init(@in);
            pinSel.Init(sel);

            chip.Simulate(cycle);

            Assert.Equal(a, pinA.GetBit(cycle));
            Assert.Equal(b, pinB.GetBit(cycle));
        }
    }
}
