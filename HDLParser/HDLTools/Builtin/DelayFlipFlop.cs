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

        public DelayFlipFlop() : this("")
        { }

        public DelayFlipFlop(string fullyQualifiedParent) : base("DFF", fullyQualifiedParent)
        {
            pinIn = new Pin(new PinDescription("in"), isOutput: false, this.FullyQualifiedName);
            pinOut = new Pin(new PinDescription("out"), isOutput: true, this.FullyQualifiedName);

            this.Pins.Add(pinIn);
            this.Pins.Add(pinOut);
        }

        public override void Simulate(int cycle)
        {
            if (cycle == 0)
            {
                pinOut.SetBit(0, -1);
            }
            else
            {
                var previousValue = pinIn.GetBit(cycle - 1);
                pinOut.SetBit(cycle, previousValue);
            }

            base.Simulate(cycle);
        }

        private class DelayFlipFlopPin : Pin
        {
            public DelayFlipFlopPin(PinDescription description, bool isOutput, string fullyQualifiedParent) : base(description, isOutput, fullyQualifiedParent, isInternal:false)
            {
            }

            //public override int[] GetValue(int cycle)
            //{
            //    if (Values.ContainsKey(cycle))
            //    {
            //        return Values[cycle];
            //    }

            //    return new[] { -1 };
            //}

            //public override void Simulate(int cycle)
            //{
            //    if(cycle == 0)
            //        this.Values[0]
            //}
        }
    }
}
