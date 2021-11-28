using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDLTools.Builtin
{
    public class Nand : Chip
    {
        private Pin pinA = new Pin("a");
        private Pin pinB = new Pin("b");
        private Pin pinOutput = new Pin("out") { IsOutput = true };

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
            var result = valueA + valueB < 2 ? 1 : 0;
            pinOutput.Values[cycle] = result;
        }
    }
}
