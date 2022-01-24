using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDLTools
{
    public class Chip
    {
        public string Name { get; set; }
        
        public List<Pin> Pins = new List<Pin>();
        private List<Chip> parts = new List<Chip>();

        protected Chip(string name)
        {
            this.Name = name;
        }

        public Chip(ChipDescription chipDescription, ChipLibrary chipLibrary)
        {
            this.Name = chipDescription.Name;

            foreach(var inputPinDescription in chipDescription.InputPins)
            {
                var pin = new Pin(inputPinDescription.Name);
                Pins.Add(pin);
            }

            foreach (var outputPinDescription in chipDescription.OutputPins)
            {
                var pin = new Pin(outputPinDescription.Name);
                pin.IsOutput = true;
                Pins.Add(pin);
            }

            foreach (PartDescription partDescription in chipDescription.Parts)
            {                
                var child = chipLibrary.GetChip(partDescription.Name);

                foreach (PinAssignmentDescription pinAssignment in partDescription.PinAssignments)
                {
                    var leftPin = child.Pins.Single(x => x.Name == pinAssignment.Left);
                    var rightPin = this.Pins.SingleOrDefault(x => x.Name == pinAssignment.Right);

                    if(rightPin == null)
                    {
                        rightPin = new Pin(pinAssignment.Right);
                        Pins.Add(rightPin);
                    }


                    if (leftPin.IsOutput)
                        rightPin.Target = leftPin;
                    else
                        leftPin.Target = rightPin;
                }
                parts.Add(child);
            }
        }

        public virtual void Simulate(int cycle)
        {
            foreach(var part in parts)
            {
                part.Simulate(cycle);
            }
        }

        public void DumpTree(StringBuilder builder, int cycle, string indent)
        {
            builder.AppendLine($"{indent}{this.Name}:");
            indent = indent + "\t";

            foreach (var pin in Pins)
            {
                builder.AppendLine($"{indent}{pin.Name}:{pin.GetValue(cycle)}");
            }

            foreach (var part in parts)
            {
                part.DumpTree(builder, cycle, indent);
            }            
        }
    }

    public class Pin
    {
        private Pin? target;
        public Dictionary<int, int> Values { get; private set; }

        public bool IsOutput { get; set; }
        public string Name { get; set; }
        //public List<Pin> Connections { get; set; }

        public Pin? Target
        {
            get
            {
                return target;
            }
            set
            {
                if (target != null)
                    throw new Exception("Why are we changing the target of a pin?");
                if (value == null)
                    throw new Exception("Cannot set a pin target to null");
                target = value;
            }
        }

        public Pin(string name)
        {
            this.Name = name;
            this.Values = new Dictionary<int, int>();
        }

        public int GetValue(int cycle)
        {
            if (Values.ContainsKey(cycle))
            {
                return Values[cycle];
            }
            else
            {
                if (this.Target == null)
                    throw new Exception("why does this pin have no target?");

                var result = this.Target.GetValue(cycle);
                Values[cycle] = result;
                return result;
            }
        }

    }


}
