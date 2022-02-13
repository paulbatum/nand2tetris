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
    public class Or16Tests
    {
        private class MyTestData : BinaryTestData
        {
            public MyTestData() : base("Or16.cmp")
            { }
        }

        private readonly ITestOutputHelper testOutput;
        public Or16Tests(ITestOutputHelper output)
        {
            this.testOutput = output;
        }

        [Theory]
        [ClassData(typeof(MyTestData))]
        public void BasicOr16(int[] a, int[] b, int[] outValue)
        {
            var library = new ChipLibrary();
            library.Register(HDLParser.ParseString(File.ReadAllText(@"hdl\Not.hdl")).Single());
            library.Register(HDLParser.ParseString(File.ReadAllText(@"hdl\And.hdl")).Single());
            library.Register(HDLParser.ParseString(File.ReadAllText(@"hdl\Or.hdl")).Single());

            var hdl = File.ReadAllText(@"hdl\Or16.hdl");
            ChipDescription desc = HDLParser.ParseString(hdl).Single();
            Chip chip = new Chip(desc, library);

            var pinA = chip.Pins.Single(x => x.Name == "a");
            var pinB = chip.Pins.Single(x => x.Name == "b");
            var pinOut = chip.Pins.Single(x => x.Name == "out");
            pinA.Init(a);
            pinB.Init(b);

            chip.Evaluate();

            Assert.Equal(outValue, pinOut.GetValue());
        }
    }
}
