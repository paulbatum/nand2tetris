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
    public class Mux16Tests
    {
        private class Mux16TestData : BinaryTestData
        {
            public Mux16TestData() : base("Mux16.cmp")
            { }
        }

        private readonly ITestOutputHelper output;
        public Mux16Tests(ITestOutputHelper output)
        {
            this.output = output;
        }
        
        [Theory]
        [ClassData(typeof(Mux16TestData))]
        public void BasicMux16(int[] a, int[] b, int sel, int[] outValue)
        {
            var library = new ChipLibrary();
            library.Register(HDLParser.ParseString(File.ReadAllText(@"hdl\And.hdl")).Single());
            library.Register(HDLParser.ParseString(File.ReadAllText(@"hdl\Or.hdl")).Single());
            library.Register(HDLParser.ParseString(File.ReadAllText(@"hdl\Not.hdl")).Single());
            library.Register(HDLParser.ParseString(File.ReadAllText(@"hdl\Mux.hdl")).Single());

            var hdl = File.ReadAllText(@"hdl\Mux16.hdl");
            ChipDescription desc = HDLParser.ParseString(hdl).Single();

            Chip chip = new Chip(desc, library);

            var cycle = 0;
            var pinA = chip.Pins.Single(x => x.Name == "a");
            var pinB = chip.Pins.Single(x => x.Name == "b");
            var pinSel = chip.Pins.Single(x => x.Name == "sel");
            var pinOut = chip.Pins.Single(x => x.Name == "out");

            pinA.Init(a);
            pinB.Init(b);
            pinSel.Init(sel);            

            chip.Simulate(cycle);

            var output = pinOut.GetValue(cycle);
            Assert.Equal(outValue, output);
        }
    }
}
