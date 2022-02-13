using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDLTools.Builtin
{
    public class DelayFlipFlop : Chip
    {
        private Pin pinIn;
        private Pin pinOut;
        private int state;

        public DelayFlipFlop() : this("")
        { }

        public DelayFlipFlop(string fullyQualifiedParent) : base("DFF", fullyQualifiedParent)
        {
            pinIn = new Pin(new PinDescription("in"), isOutput: false, this.FullyQualifiedName);
            pinOut = new Pin(new PinDescription("out"), isOutput: true, this.FullyQualifiedName);

            this.Pins.Add(pinIn);
            this.Pins.Add(pinOut);
        }

        public override void Tick()
        {
            this.state = pinIn.GetBit();
        }

        public override void Tock()
        {
            pinOut.UpdateInternalBit(state, -1); // tocks dont have a generation.. this will probably come back to bite me later
        }
    }
}
