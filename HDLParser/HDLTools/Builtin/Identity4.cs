using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDLTools.Builtin
{
    public class Identity4 : Chip
    {
        public const int Size = 4;
        private Pin inPin = new Pin(new PinDescription("in", Size), isOutput: false);
        private Pin outPin = new Pin(new PinDescription("out", Size), isOutput: true);
        public Identity4() : base("Identity4")
        {
            this.Pins.Add(inPin);
            this.Pins.Add(outPin);
        }

        public override void Simulate(int cycle)
        {
            var result = new int[Size];
            inPin.GetValue(cycle).CopyTo(result, 0);
            outPin.SetValue(cycle, result);
        }
    }
}
