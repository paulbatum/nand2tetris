using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDLTools.Builtin
{
    public class Identity4 : Chip1
    {
        public const int Size = 4;
        private Pin1 inPin;
        private Pin1 outPin;

        public Identity4() : this("")
        { }

        public Identity4(string fullyQualifiedParent) : base("Identity4", fullyQualifiedParent)
        {

            inPin = new Pin1(new PinDescription("in", Size), isOutput: false, this.FullyQualifiedName);
            outPin = new Pin1(new PinDescription("out", Size), isOutput: true, this.FullyQualifiedName);

            this.Pins.Add(inPin);
            this.Pins.Add(outPin);
        }

        internal override void Evaluate(int generation)
        {
            var temp = inPin.GetInternalValue(generation);
            outPin.UpdateInternalValue(temp, generation);
        }
    }
}
