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
    public class Not16Tests
    {
        private class MyTestData : BinaryTestData
        {
            public MyTestData() : base("Not16.cmp")
            { }
        }

        private readonly ITestOutputHelper testOutput;
        public Not16Tests(ITestOutputHelper output)
        {
            this.testOutput = output;
        }

        [Theory]
        [ClassData(typeof(MyTestData))]
        public void BasicNot16(int[] inValue, int[] outValue)
        {
            var library = new ChipLibrary();
            library.Register(HDLParser.ParseString(File.ReadAllText(@"hdl\Not.hdl")).Single());            

            var hdl = File.ReadAllText(@"hdl\Not16.hdl");
            ChipDescription desc = HDLParser.ParseString(hdl).Single();
            Chip chip = new Chip(desc, library);

            var pinIn = chip.Pins.Single(x => x.Name == "in");            
            var pinOut = chip.Pins.Single(x => x.Name == "out");
            pinIn.Init(inValue);

            chip.Evaluate();

            Assert.Equal(outValue, pinOut.GetValue());
        }
    }
}
