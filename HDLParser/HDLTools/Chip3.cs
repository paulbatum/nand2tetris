using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDLTools
{
    public class Chip3
    {
        public string Name { get; }
        public string FullyQualifiedName { get; }
        public ChipDescription? Description { get; }
        
        public List<Chip3> Parts { get; }
        public List<PinAssignmentDescription> PinAssignments { get; }
        public List<Pin3> Pins { get; }

        protected Chip3(string name, List<PinAssignmentDescription> pinAssignments, string fullyQualifiedParent)
        {
            Parts = new List<Chip3>();
            Pins = new List<Pin3>();
            this.PinAssignments = pinAssignments;
            this.Name = name;
            this.FullyQualifiedName = $"{fullyQualifiedParent}/{Name}";            
        }

        protected Chip3(ChipDescription description, ChipLibrary library, List<PinAssignmentDescription> pinAssignments, string fullyQualifiedParent) 
            : this(description.Name, pinAssignments, fullyQualifiedParent)
        {
            this.Description = description;

            foreach (var pin in description.InputPins)
            {
                Pins.Add(new InputPin3(pin.Name, this.FullyQualifiedName, pin.Width));
            }

            foreach (var pin in description.OutputPins)
            {
                Pins.Add(new OutputPin3(pin.Name, this.FullyQualifiedName, pin.Width));
            }

            foreach (var part in description.Parts)
            {
                if (part.Name == "Nand")
                {
                    var child = new Nand3(this.FullyQualifiedName, part.PinAssignments);
                    Parts.Add(child);
                }
                else
                {
                    var child = new Chip3(library.GetDescription(part.Name), library, part.PinAssignments, this.FullyQualifiedName);
                    Parts.Add(child);

                    foreach (var pinAssignment in part.PinAssignments)
                    {
                        if (Pins.Any(x => x.Name == pinAssignment.Right.Name) == false)
                        {
                            Pins.Add(new InternalPin3(pinAssignment.Right.Name, this.FullyQualifiedName));
                        }
                    }
                }
            }
            
        }

        public static Chip3 Build(ChipDescription description, ChipLibrary library)
        {
            var chip = new Chip3(description, library, new List<PinAssignmentDescription>(), "");
            chip.BuildGraph();
            return chip;
        }

        protected void BuildGraph()
        {
            foreach(var part in Parts)
            {
                foreach(var assignment in part.PinAssignments)
                {                    
                    var left = part.Pins.SingleOrDefault(x => x.Name == assignment.Left.Name) ?? throw new Exception("Invalid HDL");
                    var right = this.Pins.SingleOrDefault(x => x.Name == assignment.Right.Name) ?? throw new Exception("What happened to our internal pin?");

                    if(left is OutputPin3)
                    {
                        left.Link(right, new PinAssignmentDescription(Left: assignment.Right, Right: assignment.Left));
                    }   
                    else
                    {
                        right.Link(left, assignment);
                    }
                    
                }

                part.BuildGraph();
            }
        }

        public Simulator BuildSimulator()
        {
            GraphNode root = new GraphNode("root", "");
            root.Nodes.AddRange(Pins.OfType<InputPin3>());

            var sorted = TopologicalSort(root);

            List<InputPin3> simulatorInputs = Pins.OfType<InputPin3>().ToList();
            List<OutputPin3> simulatorOutputs = Pins.OfType<OutputPin3>().ToList();

            var simulator = new Simulator(simulatorInputs, simulatorOutputs, sorted);
            return simulator;           
        }

        public static List<GraphNode> TopologicalSort(GraphNode root)
        {
            static void Sort(GraphNode node, HashSet<GraphNode> discoveredNodes, Stack<GraphNode> nodeStack)
            {
                foreach (GraphNode child in node.Nodes)
                {
                    if (discoveredNodes.Contains(child) == false)
                    {
                        Sort(child, discoveredNodes, nodeStack);
                    }
                }

                discoveredNodes.Add(node);
                nodeStack.Push(node);
            }

            HashSet<GraphNode> discoveredNodes = new HashSet<GraphNode>();
            Stack<GraphNode> nodeStack = new Stack<GraphNode>();

            Sort(root, discoveredNodes, nodeStack);

            var result = new List<GraphNode>();
            var index = 0;
            while (nodeStack.Count > 0) 
            {
                var node = nodeStack.Pop();
                if (node.Name == "root")
                    continue; // skip the root node, we dont need it

                node.SimulatorIndex = index;
                result.Add(node);
                index++;
            }                

            return result;
        }
        

        public class Simulator
        {
            private ushort[] _values;
            private SimulationStep[] _steps;

            public List<SimulatorPin> Pins { get; } = new List<SimulatorPin>();

            public Simulator(List<InputPin3> inputs, List<OutputPin3> outputs, List<GraphNode> sortedPinGraph)
            {
                _values = new ushort[sortedPinGraph.Count];
                var steps = new List<SimulationStep>(sortedPinGraph.Count);

                foreach(var inputPin in inputs)
                {
                    Pins.Add(new SimulatorPin(inputPin, _values));
                }

                foreach (var outputPin in outputs)
                {
                    Pins.Add(new SimulatorPin(outputPin, _values));
                }

                //for(int i = 0; i < sortedPinGraph.Count; i++)
                //{
                //    var node = sortedPinGraph[i];

                //    var s = node switch
                //    {
                //        Nand3.NandOutputPin3 nandOutputPin => new SimulationStep(
                //            nandOutputPin,
                //            IsNand: true,
                //            SourceA: nandOutputPin.ParentPart.PinA.SimulatorIndex,
                //            SourceB: nandOutputPin.ParentPart.PinB.SimulatorIndex,
                //            Target: nandOutputPin.SimulatorIndex),
                //        InputPin3 p when p.SourcePin == null => new SimulationStep(
                //            p,
                //            IsNand: false,
                //            SourceA: p.SimulatorIndex,
                //            SourceB: -1,
                //            Target: p.SimulatorIndex),
                //        Pin3 p when p.SourcePin != null => new SimulationStep(
                //            p,
                //            IsNand: false,
                //            SourceA: p.SourcePin.SimulatorIndex,
                //            SourceB: 0,
                //            Target: p.SimulatorIndex,
                //            TruncateLeft: p.SourceReference!.IsIndexed ? 15 - p.SourceReference.EndIndex : 16 - p.Width,
                //            ShiftRight: p.SourceReference!.IsIndexed ? p.SourceReference.StartIndex : 0
                //            ),
                //        _ => throw new Exception("Unrecognized node")                        
                //    };

                //    steps.Add(s);
                //}

                foreach(var node in sortedPinGraph)
                {
                    var s = node.GenerateSimulationSteps();
                    steps.AddRange(s);
                }

                _steps = steps.ToArray();
                
            }

            public void Simulate()
            {
                for(int i = 0; i < _steps.Length; i++)
                {
                    var step = _steps[i];
                    if(step.IsNand)
                    {
                        _values[step.Target] = (ushort)~(_values[step.SourceA] & _values[step.SourceB]);
                    }
                    else
                    {
                        ushort v = _values[step.SourceA];
                        v = (ushort)(v << step.TruncateLeft);
                        v = (ushort)(v >> step.TruncateLeft);
                        v = (ushort)(v >> step.ShiftRight);
                        _values[step.Target] = v;
                    }
                }
            }            
        }

        public readonly record struct SimulationStep(GraphNode Node, bool IsNand, int SourceA, int SourceB, int Target, int TruncateLeft = 0, int ShiftRight = 0);

        public class SimulatorPin
        {
            public string Name { get; }
            private int _index;
            private ushort[] _values;

            public SimulatorPin(GraphNode node, ushort[] values)
            {
                Name = node.Name;
                _index = node.SimulatorIndex;
                _values = values;
            }

            public ushort Value
            {
                get
                {
                    return _values[_index];
                }
                set
                {
                    _values[_index] = value;
                }
            }
        }

        

        //public void DumpTree(StringBuilder builder, string indent)
        //{
        //    builder.AppendLine($"{indent}{this.Name}:");
        //    indent = indent + "\t";

        //    foreach (var pin in Pins)
        //    {
        //        var valueString = pin.Dump();
        //        builder.AppendLine($"{indent}{pin.Name}:{valueString}");
        //    }

        //    foreach (var part in Parts)
        //    {
        //        part.DumpTree(builder, indent);
        //    }
        //}

        public class Nand3 : Chip3
        {
            public NandInputPin3 PinA { get; }
            public NandInputPin3 PinB { get; }
            public NandOutputPin3 PinOut { get; }
            public Nand3(string fullyQualifiedParent, List<PinAssignmentDescription> pinAssignments) : base("Nand", pinAssignments, fullyQualifiedParent)
            {
                this.PinA = new NandInputPin3("a", this);
                this.PinB = new NandInputPin3("b", this);
                this.PinOut = new NandOutputPin3("out", this);

                this.PinA.Link(this.PinOut, null);
                this.PinB.Link(this.PinOut, null);

                this.Pins.Add(this.PinA);
                this.Pins.Add(this.PinB);
                this.Pins.Add(this.PinOut);
            }

            public class NandInputPin3 : InputPin3
            {
                public Nand3 Parent { get; }

                public NandInputPin3(string name, Nand3 parent) : base(name, parent.FullyQualifiedName, 1)
                {
                    this.Parent = parent;
                }

                public override void Link(Pin3 left, PinAssignmentDescription? pinAssignment)
                {
                    base.Link(left, pinAssignment);
                }
            }

            public class NandOutputPin3 : OutputPin3
            {
                public Nand3 ParentPart { get; }

                public NandOutputPin3(string name, Nand3 parent) : base(name, parent.FullyQualifiedName, 1)
                {
                    this.ParentPart = parent;
                }

                public override void Link(Pin3 left, PinAssignmentDescription? pinAssignment)
                {
                    base.Link(left, pinAssignment);
                }
            }
        }

        public class GraphNode
        {
            public string Name { get; }
            public string FullyQualifiedName { get; }
            public List<GraphNode> Nodes { get; }
            public int SimulatorIndex { get; set; }

            public GraphNode(string name, string fullyQualifiedParent)
            {
                this.Name = name;
                this.FullyQualifiedName = $"{fullyQualifiedParent}/{name}";
                this.Nodes = new List<GraphNode>();
            }

            public override string ToString()
            {
                return FullyQualifiedName;
            }

            // move this to Pin3 later
            public virtual IEnumerable<SimulationStep> GenerateSimulationSteps()
            {
                throw new Exception("can't happen");
            }
        }

        public abstract class Pin3 : GraphNode
        {
            public Pin3? SourcePin { get; private set; }
            public PinReference? SourceReference { get; private set; }
            public int Width { get; private set; }
            public Pin3(string name, string fullyQualifiedParent, int width) : base(name, fullyQualifiedParent)
            {
                this.Width = width;
            }

            public virtual void Link(Pin3 left, PinAssignmentDescription? pinAssignment)
            {                
                // come back to indexing
                this.Nodes.Add(left);
                left.SourcePin = this;

                if(pinAssignment != null)                
                    left.SourceReference = pinAssignment.Right;
            }

            public override IEnumerable<SimulationStep> GenerateSimulationSteps()
            {

            }
        }

        public class InputPin3 : Pin3
        {
            public InputPin3(string name, string fullyQualifiedParent, int width) : base(name, fullyQualifiedParent, width)
            {
            }
        }

        public class OutputPin3 : Pin3
        {
            public OutputPin3(string name, string fullyQualifiedParent, int width) : base(name, fullyQualifiedParent, width)
            {
            }

            //public override void Link(Pin3 left, PinAssignmentDescription pinAssignment)
            //{
            //    left.Link(this, new PinAssignmentDescription(Left: pinAssignment.Right, Right: pinAssignment.Left));
            //}
        }

        public class InternalPin3 : Pin3
        {
            // is it safe to treat all internal pins as full width?
            public InternalPin3(string name, string fullyQualifiedParent) : base(name, fullyQualifiedParent, 16)
            {
            }
        }


    }


}
