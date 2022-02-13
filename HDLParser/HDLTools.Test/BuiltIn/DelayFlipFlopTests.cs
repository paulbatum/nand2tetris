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
        public void IsRegistered()
        {
            var chipLibrary = new ChipLibrary();
            var chip = chipLibrary.GetChip("DFF", "");
            Assert.NotNull(chip);
        }

        [Fact]
        public void ValueIsPreservedAfterTock()
        {
            var chip = new DelayFlipFlop();

            var pinIn = chip.Pins.Single(x => x.Name == "in");            
            var pinOut = chip.Pins.Single(x => x.Name == "out");
            
            pinIn.Init(1);
            chip.Tick();
            chip.Tock();

            Assert.Equal(1, pinOut.GetBit());
        }

        [Fact]
        public void ValueIsNotPreservedByTick()
        {
            var chip = new DelayFlipFlop();

            var pinIn = chip.Pins.Single(x => x.Name == "in");
            var pinOut = chip.Pins.Single(x => x.Name == "out");

            pinIn.Init(1);
            chip.Tick();
            chip.Tock();

            Assert.Equal(1, pinOut.GetBit());

            pinIn.Init(0);
            chip.Tick();
            Assert.Equal(1, pinOut.GetBit());

            chip.Tock();
            Assert.Equal(0, pinOut.GetBit());
        }
    }
}
