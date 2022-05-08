using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace HDLTools.Test.Chapter1
{
    public class MuxTests
    {
        private class MuxTestData : BinaryTestData
        {
            public MuxTestData() : base("Mux.cmp")
            { }
        }

        [Theory]
        [ClassData(typeof(MuxTestData))]
        public void BasicMux(int a, int b, int sel, int outValue)
        {
            var library = new ChipLibrary();
            library.Register(HDLParser.ParseString(File.ReadAllText(@"hdl\And.hdl")).Single());
            library.Register(HDLParser.ParseString(File.ReadAllText(@"hdl\Or.hdl")).Single());
            library.Register(HDLParser.ParseString(File.ReadAllText(@"hdl\Not.hdl")).Single());

            var hdl = File.ReadAllText(@"hdl\Mux.hdl");
            ChipDescription desc = HDLParser.ParseString(hdl).Single();
            Chip chip = new Chip(desc, library);

            var pinA = chip.Pins.Single(x => x.Name == "a");
            var pinB = chip.Pins.Single(x => x.Name == "b");
            var pinSel = chip.Pins.Single(x => x.Name == "sel");
            var pinOut = chip.Pins.Single(x => x.Name == "out");
            pinA.Init(a);
            pinB.Init(b);
            pinSel.Init(sel);

            chip.Evaluate();

            Assert.Equal(outValue, pinOut.GetBit());
        }

        [Theory]
        [ClassData(typeof(MuxTestData))]
        public void Chip3Mux(int a, int b, int sel, int outValue)
        {
            var library = new ChipLibrary();
            library.Register(HDLParser.ParseString(File.ReadAllText(@"hdl\And.hdl")).Single());
            library.Register(HDLParser.ParseString(File.ReadAllText(@"hdl\Or.hdl")).Single());
            library.Register(HDLParser.ParseString(File.ReadAllText(@"hdl\Not.hdl")).Single());

            var hdl = File.ReadAllText(@"hdl\Mux.hdl");
            ChipDescription desc = HDLParser.ParseString(hdl).Single();

            var chip = Chip3.Build(desc, library);

            var sim = chip.BuildSimulator();
            var pinA = sim.Pins.Single(x => x.Name == "a");
            var pinB = sim.Pins.Single(x => x.Name == "b");
            var pinSel = sim.Pins.Single(x => x.Name == "sel");
            var pinOut = sim.Pins.Single(x => x.Name == "out");

            pinA.Value = (ushort) a;
            pinB.Value = (ushort) b;
            pinSel.Value = (ushort) sel;

            sim.Simulate();

            Assert.Equal(outValue, pinOut.Value);
        }
    }
}
