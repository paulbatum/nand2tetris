using HDLTools.Builtin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace HDLTools.Test.BuiltIn
{
    public class DelayFlipFlopTests
    {
        private readonly ITestOutputHelper testOutput;
        public DelayFlipFlopTests(ITestOutputHelper output)
        {
            this.testOutput = output;
        }

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

        [Fact]
        public void CanHaveCycles()
        {
            var library = new ChipLibrary();
            library.LoadAll("hdl");

            var cycleHDL =
                @"CHIP Oscilator {                    
                    IN in, load;
                    OUT out;

                    PARTS:
                    Mux(a=last, b=in, sel=load, out=out1);
                    Not(in=out1,out=notout1);                    
                    DFF(in=notout1, out=last, out=out);
                }";

            ChipDescription cycleDescription = HDLParser.ParseString(cycleHDL).Single();
            Chip chip = new Chip(cycleDescription, library);

            var pinIn = chip.Pins.Single(x => x.Name == "in");
            var pinLoad = chip.Pins.Single(x => x.Name == "load");
            var pinOut = chip.Pins.Single(x => x.Name == "out");
            
            pinIn.Init(0);
            pinLoad.Init(1);

            chip.Tick();
            chip.Tock();

            Assert.Equal(1, pinOut.GetBit());            
            pinLoad.Init(0);            

            chip.Tick();
            chip.Tock();

            Assert.Equal(0, pinOut.GetBit());

            chip.Tick();
            chip.Tock();

            Assert.Equal(1, pinOut.GetBit());

            chip.Tick();
            chip.Tock();

            Assert.Equal(0, pinOut.GetBit());

            // reset using a 1

            pinIn.Init(1);
            pinLoad.Init(1);

            chip.Tick();
            chip.Tock();

            Assert.Equal(0, pinOut.GetBit());
            pinLoad.Init(0);

            chip.Tick();
            chip.Tock();

            Assert.Equal(1, pinOut.GetBit());

            chip.Tick();
            chip.Tock();

            Assert.Equal(0, pinOut.GetBit());

            chip.Tick();
            chip.Tock();

            Assert.Equal(1, pinOut.GetBit());
        }

    }
}
