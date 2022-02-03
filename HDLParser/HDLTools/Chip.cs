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
                        // this logic is duplicated from the connection code.. need to consider a refactoring
                        var internalPinSize = pinAssignment.Left.IsIndexed ? (pinAssignment.Left.EndIndex - pinAssignment.Left.StartIndex) + 1 : leftPin.Width;
                        var internalPinDescription = new PinDescription(pinAssignment.Right.Name, internalPinSize);
                        rightPin = new Pin(internalPinDescription, isOutput: false);
                        Pins.Add(rightPin);
                    }

                    if (leftPin.IsOutput)
                        rightPin.AddConnection(leftPin, pinAssignment.Left, pinAssignment.Right);
                    else
                        leftPin.AddConnection(rightPin, pinAssignment.Right, pinAssignment.Left);
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

        public virtual void Invalidate(int cycle)
        {
            foreach (var part in parts)
            {
                part.Invalidate(cycle);
            }
        }

        public void DumpTree(StringBuilder builder, int cycle, string indent)
        {
            builder.AppendLine($"{indent}{this.Name}:");
            indent = indent + "\t";

            foreach (var pin in Pins)
            {
                var val = string.Join("", pin.GetValue(cycle));
                builder.AppendLine($"{indent}{pin.Name}:{val}");
            }

            foreach (var part in parts)
            {
                part.DumpTree(builder, cycle, indent);
            }            
        }
    }

    public class Pin
    {
        private List<Connection> connections = new List<Connection>();

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

        public void AddConnection(Pin target, PinReference targetReference, PinReference myReference)
        {
            if (target == null)
                throw new Exception("Cannot set a pin target to null");

            var targetSize = targetReference.IsIndexed ? (targetReference.EndIndex - targetReference.StartIndex) + 1 : target.Width;
            var mySize = myReference.IsIndexed ? (myReference.EndIndex - myReference.StartIndex) + 1 : this.Width;

            if (targetSize != mySize)
                throw new Exception($"Connection sizes don't match. Target '{target.Name}' size is '{targetSize}', my '{myReference.Name}' size is '{mySize}'.");

            //if (targetIndexEnd > target.Width - 1)
            //    throw new Exception($"The index {targetIndexEnd} is out of range for target pin '{target.Name}' of width={target.Width}.");

            var connection = new Connection(target,
                targetReference.IsIndexed ? targetReference.StartIndex : 0,
                myReference.IsIndexed ? myReference.StartIndex : 0,
                mySize);

            this.connections.Add(connection);
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

                foreach (var c in connections)
                    c.Apply(result, cycle);

                Values[cycle] = result;
                return result;
            }
        }

        public void SetValue(int cycle, int[] values)
        {
            this.Values[cycle] = values;
        }

        public int GetBit(int cycle)
        {
            var result = this.GetValue(cycle);

            if (result.Length != 1)
                throw new Exception("GetBit only supported on pins with width=1");

            return result[0];
        }

        public void SetBit(int cycle, int value)
        {
            if (this.Width != 1)
                throw new Exception("SetBit only supported on pins with width=1");

            SetValue(cycle, new[] { value });
        }

        public void Init(int value)
        {
            SetBit(0, value);
        }

        public void Init(int[] values)
        {
            this.Values[0] = values;
        }

        public void Invalidate(int cycle)
        {
            this.Values.Remove(cycle);
        }

        private record Connection(Pin Target, int TargetStartIndex, int MyStartIndex, int Width)
        {
            public void Apply(int[] myState, int cycle)
            {
                // indexers are right to left, e.g. for sel=110, sel[2]=1, sel[1]=1, sel[0]=0
                // so we have to start from the right which is width - 1

                var targetState = Target.GetValue(cycle);
                var myStartingPoint = myState.Length - 1 - MyStartIndex;
                var targetStartingPoint = targetState.Length - 1 - TargetStartIndex;

                for (int i = 0; i < Width; i++)
                {
                    myState[myStartingPoint - i] = targetState[targetStartingPoint - i];
                }
            }
        }
    }
}
