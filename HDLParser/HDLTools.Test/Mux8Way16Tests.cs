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
    public class Mux8Way16Tests
    {
        private class MyTestData : BinaryTestData
        {
            public MyTestData() : base("Mux8Way16.cmp")
            { }
        }

        private readonly ITestOutputHelper testOutput;
        public Mux8Way16Tests(ITestOutputHelper output)
        {
            this.testOutput = output;
        }
        
        [Theory]
        [ClassData(typeof(MyTestData))]
        public void BasicMux8Way16(int[] a, int[] b, int[] c, int[] d, int[] e, int[] f, int[] g, int[] h, int[] sel, int[] outValue)
        {
            var library = new ChipLibrary();
            library.Register(HDLParser.ParseString(File.ReadAllText(@"hdl\And.hdl")).Single());
            library.Register(HDLParser.ParseString(File.ReadAllText(@"hdl\Or.hdl")).Single());
            library.Register(HDLParser.ParseString(File.ReadAllText(@"hdl\Not.hdl")).Single());
            library.Register(HDLParser.ParseString(File.ReadAllText(@"hdl\Mux.hdl")).Single());
            library.Register(HDLParser.ParseString(File.ReadAllText(@"hdl\Mux16.hdl")).Single());
            library.Register(HDLParser.ParseString(File.ReadAllText(@"hdl\Mux4Way16.hdl")).Single());

            var hdl = File.ReadAllText(@"hdl\Mux8Way16.hdl");
            ChipDescription desc = HDLParser.ParseString(hdl).Single();

            Chip chip = new Chip(desc, library);

            var cycle = 0;
            var inputPins = "abcdefgh"
                .ToCharArray()
                .Select(x => chip.Pins.Single(p => p.Name == x.ToString()))
                .ToArray();
            var pinSel = chip.Pins.Single(x => x.Name == "sel");
            var pinOut = chip.Pins.Single(x => x.Name == "out");

            pinSel.Init(sel);                        

            var inputValues = new[] { a, b, c, d, e, f, g, h };

            for (int index = 0; index < inputValues.Length; index++)
            {
                inputPins[index].Init(inputValues[index]);
            }

            chip.Simulate(cycle);

            Assert.Equal(outValue, pinOut.GetValue(cycle));
        }
    }
}
