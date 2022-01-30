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
                var pin = new Pin(inputPinDescription, isOutput: false);
                Pins.Add(pin);
            }

            foreach (var outputPinDescription in chipDescription.OutputPins)
            {
                var pin = new Pin(outputPinDescription, isOutput: true);
                Pins.Add(pin);
            }

            foreach (PartDescription partDescription in chipDescription.Parts)
            {                
                var child = chipLibrary.GetChip(partDescription.Name);

                foreach (PinAssignmentDescription pinAssignment in partDescription.PinAssignments)
                {
                    var leftPin = child.Pins.Single(x => x.Name == pinAssignment.Left.Name);
                    var rightPin = this.Pins.SingleOrDefault(x => x.Name == pinAssignment.Right.Name);

                    if(rightPin == null)
                    {
                        var intermediatePinDescription = new PinDescription(pinAssignment.Right.Name, 1);
                        rightPin = new Pin(intermediatePinDescription, isOutput: false); // is isOutput always false here?
                        Pins.Add(rightPin);
                    }

                    if (leftPin.IsOutput)
                        rightPin.AddConnection(leftPin, pinAssignment.Left.Index, pinAssignment.Right.Index);
                    else
                        leftPin.AddConnection(rightPin, pinAssignment.Right.Index, pinAssignment.Left.Index);
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
        private List<Connection> connections = new List<Connection> ();

        public Dictionary<int, int[]> Values { get; private set; }

        public bool IsOutput { get; private set; }
        public string Name { get; private set; }
        public int Width { get; private set; }

        public Pin(PinDescription description, bool isOutput)
        {
            this.Name = description.Name;
            this.Width = description.Width;
            this.IsOutput = isOutput;
            
            this.Values = new Dictionary<int, int[]>();
        }        

        public void AddConnection(Pin target, int targetIndex, int myIndex)
        {
            if (target == null)
                throw new Exception("Cannot set a pin target to null");

            if (targetIndex > target.Width - 1)
                throw new Exception($"The index {targetIndex} is out of range for target pin {target.Name} of width={target.Width}.");

            if (this.connections.Any(x => x.Index == myIndex))
                throw new Exception($"Why are we adding a connection for which there is already one?");

            this.connections.Add(new Connection(target, targetIndex, myIndex));
        }

        public int[] GetValue(int cycle)
        {
            if (Values.ContainsKey(cycle))
            {
                return Values[cycle];
            }
            else
            {
                int[] result = new int[Width];

                foreach(var c in connections)
                {
                    var val = c.Target.GetValue(cycle);
                    result[c.Index] = val[c.TargetIndex];
                }
                
                Values[cycle] = result;
                return result;
            }
        }        

        public int GetBit(int cycle)
        {
            var result = this.GetValue(cycle);

            if(result.Length != 1)
                throw new Exception("GetBit only supported on pins with width=1");

            return result[0];
        }

        public void SetBit(int cycle, int value)
        {
            if (this.Width != 1)
                throw new Exception("SetBit only supported on pins with width=1");

            this.Values[cycle] = new[] { value };
        }

        public void Init(int value)
        {
            SetBit(0, value);
        }

        public void Init(int[] values)
        {
            this.Values[0] = values;
        }

        private class Connection
        {
            public Pin Target { get; set; }
            public int TargetIndex { get; set; }
            public int Index { get; set; }

            public Connection(Pin target, int targetIndex, int index)
            {
                this.Target = target;
                this.TargetIndex = targetIndex;
                this.Index = index;
            }
        }
    }
}
