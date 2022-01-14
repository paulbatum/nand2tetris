using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace HDLTools.Test
{
    public class MuxTests
    {
        private class MuxTestData : ComparisonTestData
        {
            public MuxTestData() : base("Mux.cmp")
            { }
        }

        [Theory]
        [ClassData(typeof(MuxTestData))]
        public void BasicMux(int a, int b, int sel, int outValue)
        {
            var library = new ChipLibrary();
            library.Register(HDLParser.ParseString(File.ReadAllText("And.hdl")).Single());
            library.Register(HDLParser.ParseString(File.ReadAllText("Or.hdl")).Single());
            library.Register(HDLParser.ParseString(File.ReadAllText("Not.hdl")).Single());

            var hdl = File.ReadAllText("Mux.hdl");
            ChipDescription desc = HDLParser.ParseString(hdl).Single();

            Chip chip = new Chip(desc, library);

            var cycle = 0;
            var pinA = chip.Pins.Single(x => x.Name == "a");
            var pinB = chip.Pins.Single(x => x.Name == "b");
            var pinSel = chip.Pins.Single(x => x.Name == "sel");
            var pinOut = chip.Pins.Single(x => x.Name == "out");
            pinA.Values[cycle] = a;
            pinB.Values[cycle] = b;
            pinSel.Values[cycle] = sel;

            chip.Simulate(cycle);

            Assert.Equal(outValue, pinOut.GetValue(cycle));
        }
    }
}
