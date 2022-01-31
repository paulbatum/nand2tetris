using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace HDLTools.Test.Chapter1
{
    public class DMux8WayTests
    {
        private class DMux8WayTestData : BinaryTestData
        {
            public DMux8WayTestData() : base("DMux8Way.cmp")
            { }
        }

        private readonly ITestOutputHelper testOutput;
        public DMux8WayTests(ITestOutputHelper output)
        {
            this.testOutput = output;
        }

        [Theory]
        [ClassData(typeof(DMux8WayTestData))]
        public void BasicDMux8Way(int @in, int[] sel, int a, int b, int c, int d, int e, int f, int g, int h)
        {
            var library = new ChipLibrary();
            library.Register(HDLParser.ParseString(File.ReadAllText(@"hdl\And.hdl")).Single());
            library.Register(HDLParser.ParseString(File.ReadAllText(@"hdl\Not.hdl")).Single());
            library.Register(HDLParser.ParseString(File.ReadAllText(@"hdl\DMux.hdl")).Single());
            library.Register(HDLParser.ParseString(File.ReadAllText(@"hdl\DMux4Way.hdl")).Single());

            var hdl = File.ReadAllText(@"hdl\DMux8Way.hdl");
            ChipDescription desc = HDLParser.ParseString(hdl).Single();

            Chip chip = new Chip(desc, library);

            var cycle = 0;
            var pinIn = chip.Pins.Single(x => x.Name == "in");
            var pinSel = chip.Pins.Single(x => x.Name == "sel");

            var outputPins = "abcdefgh"
                .ToCharArray()
                .Select(x => chip.Pins.Single(p => p.Name == x.ToString()))
                .ToArray();            

            pinIn.Init(@in);
            pinSel.Init(sel);

            chip.Simulate(cycle);

            var expectedValues = new[] { a, b, c, d, e, f, g, h };

            for(int index = 0; index < expectedValues.Length; index++)
            {
                Assert.Equal(expectedValues[index], outputPins[index].GetBit(cycle));
            }
        }
    }
}
