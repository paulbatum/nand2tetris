using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDLTools
{
    public class ChipGraph
    {
        public List<InputPinNode> Inputs { get; set; }
        public List<GraphNode> Outputs { get; set;}

        public ChipGraph()
        {
            Inputs = new List<InputPinNode>();
            Outputs = new List<GraphNode>();
        }

        public static ChipGraph Parse(ChipDescription description, ChipLibrary library)
        {
            
            var graph = new ChipGraph();

            foreach (var inputPin in description.InputPins)
                graph.Inputs.Add(new InputPinNode(inputPin.Name, description.Name, inputPin.Width));

            foreach (var outputPin in description.OutputPins)
                graph.Outputs.Add(new GraphNode(outputPin.Name, description.Name));            

            foreach (var part in description.Parts)
            {
                var currentDescription = library.GetDescription(part.Name);                

                foreach(var assignment in part.PinAssignments)
                {
                    
                }
            }


            return graph;
        }

        public class GraphNode
        {
            public string Name { get; }
            public string FullyQualifiedName { get; }

            public GraphNode(string name, string fullyQualifiedParent)
            {
                this.Name = name;
                this.FullyQualifiedName = $"{fullyQualifiedParent}/{name}";
            }
        }

        public class InputPinNode : GraphNode
        {
            public int Width { get; }
            public InputPinNode(string name, string fullyQualifiedParent, int width) : base(name, fullyQualifiedParent)
            {
                Width = width;
            }
        }

    }
}
