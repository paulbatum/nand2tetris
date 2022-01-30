using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDLTools.Builtin
{
    public class Nand : Chip
    {
        private Pin pinA = new Pin(new PinDescription("a"), isOutput: false);
        private Pin pinB = new Pin(new PinDescription("b"), isOutput: false);
        private Pin pinOutput = new Pin(new PinDescription("out"), isOutput: true);

        public Nand() : base("Nand")
        {            
            this.Pins.Add(pinA);
            this.Pins.Add(pinB);
            this.Pins.Add(pinOutput);                        
        }

        public override void Simulate(int cycle)
        {
            var valueA = pinA.GetValue(cycle);
            var valueB = pinB.GetValue(cycle);
            var result = valueA[0] + valueB[0] < 2 ? 1 : 0;
            pinOutput.SetBit(cycle, result);
        }
    }
}
