using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace HDLTools.Test.Chapter1
{
    public class Mux4Way16Tests
    {
        private class MyTestData : BinaryTestData
        {
            public MyTestData() : base("Mux4Way16.cmp")
            { }
        }

        private readonly ITestOutputHelper testOutput;
        public Mux4Way16Tests(ITestOutputHelper output)
        {
            this.testOutput = output;
        }
        
        [Theory]
        [ClassData(typeof(MyTestData))]
        public void BasicMux4Way16(int[] a, int[] b, int[] c, int[] d, int[] sel, int[] outValue)
        {
            var library = new ChipLibrary();
            library.Register(HDLParser.ParseString(File.ReadAllText(@"hdl\And.hdl")).Single());
            library.Register(HDLParser.ParseString(File.ReadAllText(@"hdl\Or.hdl")).Single());
            library.Register(HDLParser.ParseString(File.ReadAllText(@"hdl\Not.hdl")).Single());
            library.Register(HDLParser.ParseString(File.ReadAllText(@"hdl\Mux.hdl")).Single());
            library.Register(HDLParser.ParseString(File.ReadAllText(@"hdl\Mux16.hdl")).Single());

            var hdl = File.ReadAllText(@"hdl\Mux4Way16.hdl");
            ChipDescription desc = HDLParser.ParseString(hdl).Single();
            Chip chip = new Chip(desc, library);

            var pinA = chip.Pins.Single(x => x.Name == "a");
            var pinB = chip.Pins.Single(x => x.Name == "b");
            var pinC = chip.Pins.Single(x => x.Name == "c");
            var pinD = chip.Pins.Single(x => x.Name == "d");
            var pinSel = chip.Pins.Single(x => x.Name == "sel");
            var pinOut = chip.Pins.Single(x => x.Name == "out");

            pinA.Init(a);
            pinB.Init(b);
            pinC.Init(c);
            pinD.Init(d);
            pinSel.Init(sel);            

            chip.Evaluate();

            var output = pinOut.GetValue();
            Assert.Equal(outValue, output);
        }
    }
}
