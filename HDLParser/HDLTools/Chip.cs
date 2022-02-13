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
        private static int generationCounter = 0;

        public string Name { get; }
        public string FullyQualifiedName { get; }
        
        public List<Pin> Pins = new List<Pin>();
        private List<Chip> parts = new List<Chip>();

        protected Chip(string name, string fullyQualifiedParent)
        {
            this.Name = name;
            this.FullyQualifiedName = $"{fullyQualifiedParent}/{name}";
        }

        public Chip(ChipDescription chipDescription, ChipLibrary chipLibrary) : this(chipDescription, chipLibrary, "")
        { }

        public Chip(ChipDescription chipDescription, ChipLibrary chipLibrary, string fullyQualifiedParent) : this(chipDescription.Name, fullyQualifiedParent)
        {            
            foreach(var inputPinDescription in chipDescription.InputPins)
            {
                var pin = new Pin(inputPinDescription, isOutput: false, this.FullyQualifiedName);
                Pins.Add(pin);
            }

            foreach (var outputPinDescription in chipDescription.OutputPins)
            {
                var pin = new Pin(outputPinDescription, isOutput: true, this.FullyQualifiedName);
                Pins.Add(pin);
            }

            foreach (PartDescription partDescription in chipDescription.Parts)
            {                
                var child = chipLibrary.GetChip(partDescription.Name, this.FullyQualifiedName);

                foreach (PinAssignmentDescription pinAssignment in partDescription.PinAssignments)
                {
                    var leftPin = child.Pins.Single(x => x.Name == pinAssignment.Left.Name);
                    var rightPin = this.Pins.SingleOrDefault(x => x.Name == pinAssignment.Right.Name);

                    if(rightPin == null)
                    {
                        // this logic is duplicated from the connection code.. need to consider a refactoring
                        var internalPinSize = pinAssignment.Left.IsIndexed ? (pinAssignment.Left.EndIndex - pinAssignment.Left.StartIndex) + 1 : leftPin.Width;
                        var internalPinDescription = new PinDescription(pinAssignment.Right.Name, internalPinSize);
                        rightPin = new Pin(internalPinDescription, isOutput: false, this.FullyQualifiedName, isInternal: true);
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

        public void Evaluate()
        {
            generationCounter++;
            this.Evaluate(generationCounter);
        }

        internal virtual void Evaluate(int generation)
        {
            foreach (var part in parts)
            {
                part.Evaluate(generation);
            }

            foreach(var pin in Pins)
            {
                pin.Evaluate(generation);
            }
        }

        public virtual void Tick()
        {
            Evaluate();

            foreach (var part in parts)
            {
                part.Tick();
            }
        }

        public virtual void Tock()
        {
            foreach (var part in parts)
            {
                part.Tock();
            }

            Evaluate();
        }

        public string DumpTree(int cycle)
        {
            StringBuilder sb = new StringBuilder();
            DumpTree(sb, cycle, "");
            return sb.ToString();
        }

        public void DumpTree(StringBuilder builder, int cycle, string indent)
        {
            builder.AppendLine($"{indent}{this.Name}:");
            indent = indent + "\t";

            foreach (var pin in Pins)
            {
                var valueString = pin.Dump();
                builder.AppendLine($"{indent}{pin.Name}:{valueString}");
            }

            foreach (var part in parts)
            {
                part.DumpTree(builder, cycle, indent);
            }            
        }
    }

    public class Pin
    {
        protected List<Connection> connections = new List<Connection>();
        protected int[] internalState;
        protected int lastStateGeneration = 0;
        protected bool setExplicitly = false;

        public bool IsInternal { get; private set; }
        public bool IsOutput { get; private set; }
        public string Name { get; private set; }
        public string FullyQualifiedName { get; }
        public int Width { get; private set; }

        public Pin(PinDescription description, bool isOutput, string fullyQualifiedParent, bool isInternal = false)
        {
            this.Name = description.Name;
            this.FullyQualifiedName = $"{fullyQualifiedParent}/{Name}";
            this.Width = description.Width;
            this.IsOutput = isOutput;
            this.IsInternal = isInternal;

            this.internalState = new int[Width];
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

        internal int[] GetInternalValue(int generation)
        {
            if(this.setExplicitly || lastStateGeneration == generation)
                return this.internalState;

            //throw new Exception("Tried to get an internal value that has not been evaluated yet");
            Evaluate(generation);
            return this.internalState;
        }

        internal void UpdateInternalBit(int value, int generation)
        {
            internalState[0] = value;
            this.lastStateGeneration = generation;
        }

        internal void UpdateInternalValue(int[] values, int generation)
        {
            values.CopyTo(internalState, 0);
            this.lastStateGeneration = generation;
        }

        internal string Dump()
        {
            return $"{string.Join("", internalState)}, explicitSet={setExplicitly}, lastGen={lastStateGeneration}";
        }

        public int[] GetValue()
        {
            return internalState;
        }

        public int GetBit()
        {
            if (this.Width != 1)
                throw new Exception("GetBit only supported on pins with width=1");

            return internalState[0];
        }

        public void Init(int value)
        {
            if (this.Width != 1)
                throw new Exception("SetBit only supported on pins with width=1");

            internalState[0] = value;
            this.setExplicitly = true;
        }

        public void Init(int[] values)
        {
            if (values.Length != this.Width)
                throw new Exception("Lengths dont match");

            values.CopyTo(internalState, 0);
            this.setExplicitly = true;
        }

        internal virtual void Evaluate(int generation)
        {
            if (lastStateGeneration == generation)
                return;

            foreach (var c in connections)
                c.Apply(this.internalState, generation);

            lastStateGeneration = generation;
        }

        protected record Connection(Pin Target, int TargetStartIndex, int MyStartIndex, int Width)
        {
            public void Apply(int[] myState, int generation)
            {
                // indexers are right to left, e.g. for sel=110, sel[2]=1, sel[1]=1, sel[0]=0
                // so we have to start from the right which is width - 1

                var targetState = Target.GetInternalValue(generation);
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
