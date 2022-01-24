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
        private class Mux16TestData : ComparisonTestData
        {
            public Mux16TestData() : base("Mux16.cmp")
            { }
        }

        private readonly ITestOutputHelper output;
        public Mux16Tests(ITestOutputHelper output)
        {
            this.output = output;
        }
        
        [Theory(Skip = "not ready yet")]
        [ClassData(typeof(Mux16TestData))]
        public void BasicMux16(int[] a, int[] b, int[] outValue)
        {
            var library = new ChipLibrary();
            library.Register(HDLParser.ParseString(File.ReadAllText("And.hdl")).Single());
            library.Register(HDLParser.ParseString(File.ReadAllText("Or.hdl")).Single());
            library.Register(HDLParser.ParseString(File.ReadAllText("Not.hdl")).Single());
            library.Register(HDLParser.ParseString(File.ReadAllText("Mux.hdl")).Single());

            var hdl = File.ReadAllText("Mux16.hdl");
            ChipDescription desc = HDLParser.ParseString(hdl).Single();

            Chip chip = new Chip(desc, library);

            var cycle = 0;
            var pinA = chip.Pins.Single(x => x.Name == "a");
            var pinB = chip.Pins.Single(x => x.Name == "b");
            var pinOut = chip.Pins.Single(x => x.Name == "out");
            //pinA.Values[cycle] = a;
            //pinB.Values[cycle] = b;

            chip.Simulate(cycle);

            output.DumpChip(chip, cycle);

            //Assert.Equal(outValue, pinOut.GetValue(cycle));
        }
    }
}
