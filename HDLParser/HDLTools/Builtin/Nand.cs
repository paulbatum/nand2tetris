using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDLTools.Builtin
{
    public class Nand : Chip
    {
        private Pin pinA;
        private Pin pinB;
        private Pin pinOutput;

        public Nand() : this("")
        { }

        public Nand(string fullyQualifiedParent) : base("Nand", fullyQualifiedParent)
        {
            pinA = new Pin(new PinDescription("a"), isOutput: false, this.FullyQualifiedName);
            pinB = new Pin(new PinDescription("b"), isOutput: false, this.FullyQualifiedName);
            pinOutput = new Pin(new PinDescription("out"), isOutput: true, this.FullyQualifiedName);

            this.Pins.Add(pinA);
            this.Pins.Add(pinB);
            this.Pins.Add(pinOutput);
        }

        internal override void Evaluate(int generation)
        {
            var valueA = pinA.GetInternalValue(generation)[0];
            var valueB = pinB.GetInternalValue(generation)[0];

            var result = valueA + valueB < 2 ? 1 : 0;
            pinOutput.UpdateInternalBit(result, generation);
        }
    }
}
