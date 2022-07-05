using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDLTools.Builtin
{
    public class Nand : Chip1
    {
        private Pin1 pinA;
        private Pin1 pinB;
        private Pin1 pinOutput;

        public Nand() : this("")
        { }

        public Nand(string fullyQualifiedParent) : base("Nand", fullyQualifiedParent)
        {
            pinA = new Pin1(new PinDescription("a"), isOutput: false, this.FullyQualifiedName);
            pinB = new Pin1(new PinDescription("b"), isOutput: false, this.FullyQualifiedName);
            pinOutput = new Pin1(new PinDescription("out"), isOutput: true, this.FullyQualifiedName);

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
