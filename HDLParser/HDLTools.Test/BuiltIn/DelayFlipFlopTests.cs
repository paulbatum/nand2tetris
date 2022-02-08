using HDLTools.Builtin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace HDLTools.Test.BuiltIn
{
    public class DelayFlipFlopTests
    {
        [Fact]
        public void BasicDFF()
        {
            var chip = new DelayFlipFlop();
            var cycle = 0;

            var pinIn = chip.Pins.Single(x => x.Name == "in");            
            var pinOut = chip.Pins.Single(x => x.Name == "out");
            
            pinIn.SetBit(cycle, 0);
            Assert.Null(pinOut.GetValue(cycle)); // dff state is undefined a t = 0            

            cycle = 1;
            chip.Simulate(cycle);

            Assert.Equal(0, pinOut.GetBit(cycle));

            pinIn.SetBit(cycle, 1);
            Assert.Equal(0, pinOut.GetBit(cycle));

            cycle = 2;
            chip.Simulate(cycle);
            Assert.Equal(1, pinOut.GetBit(cycle));

            pinIn.SetBit(cycle, 0);
            Assert.Equal(1, pinOut.GetBit(cycle));

            cycle = 3;
            chip.Simulate(cycle);
            Assert.Equal(0, pinOut.GetBit(cycle));
        }

        [Fact]
        public void IsRegistered()
        {
            var chipLibrary = new ChipLibrary();
            var chip = chipLibrary.GetChip("DFF", "");
            Assert.NotNull(chip);
        }
    }
}
