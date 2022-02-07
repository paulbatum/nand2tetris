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
    public class Or8WayTests
    {
        private class MyTestData : BinaryTestData
        {
            public MyTestData() : base("Or8Way.cmp")
            { }
        }

        private readonly ITestOutputHelper testOutput;
        public Or8WayTests(ITestOutputHelper output)
        {
            this.testOutput = output;
        }

        [Theory]
        [ClassData(typeof(MyTestData))]
        public void BasicOr8Way(int[] inValue, int outValue)
        {
            var library = new ChipLibrary();
            library.Register(HDLParser.ParseString(File.ReadAllText(@"hdl\Not.hdl")).Single());
            library.Register(HDLParser.ParseString(File.ReadAllText(@"hdl\And.hdl")).Single());
            library.Register(HDLParser.ParseString(File.ReadAllText(@"hdl\Or.hdl")).Single());

            var hdl = File.ReadAllText(@"hdl\Or8Way.hdl");
            ChipDescription desc = HDLParser.ParseString(hdl).Single();

            Chip chip = new Chip(desc, library);

            var cycle = 0;
            var pinIn = chip.Pins.Single(x => x.Name == "in");
            var pinOut = chip.Pins.Single(x => x.Name == "out");
            pinIn.Init(inValue);            

            chip.Simulate(cycle);

            Assert.Equal(outValue, pinOut.GetBit(cycle));
        }
    }
}
