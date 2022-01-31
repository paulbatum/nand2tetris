using HDLTools.Builtin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace HDLTools.Test.BuiltIn
{
    public class NandTests
    {
        [Theory]
        [InlineData(0, 0, 1)]
        [InlineData(1, 1, 0)]
        [InlineData(0, 1, 1)]
        [InlineData(1, 0, 1)]
        public void BasicNand(int a, int b, int outValue)
        {
            var chip = new Nand();


            var cycle = 0;
            var pinA = chip.Pins.Single(x => x.Name == "a");
            var pinB = chip.Pins.Single(x => x.Name == "b");
            var pinOut = chip.Pins.Single(x => x.Name == "out");
            pinA.Init(a);
            pinB.Init(b);

            chip.Simulate(cycle);

            Assert.Equal(outValue, pinOut.GetBit(cycle));
        }
    }
}
