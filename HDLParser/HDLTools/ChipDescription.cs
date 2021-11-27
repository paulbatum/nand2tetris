using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDLTools
{
    public class ChipDescription
    {
        public string Name { get; set; }
        public List<PinDescription> InputPins { get; }
        public List<PinDescription> OutputPins { get; }

        public List<PartDescription> Parts { get; }

        public ChipDescription(string name, List<PinDescription> inputPins, List<PinDescription> outputPins, List<PartDescription> parts)
        {
            Name = name;
            InputPins = inputPins;
            OutputPins = outputPins;
            Parts = parts;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine($"CHIP {this.Name} {{");

            builder.Append("\tIN ");
            builder.AppendLine(string.Join(", ", InputPins.Select(x => x.Name)) + ";");

            builder.Append("\tOUT ");
            builder.AppendLine(string.Join(", ", OutputPins.Select(x => x.Name)) + ";");
            builder.AppendLine();

            builder.AppendLine("\tPARTS:");            
            foreach (var part in Parts)
            {
                builder.Append($"\t{part.Name} (");
                builder.Append(string.Join(", ", part.PinAssignments.Select(x => $"{x.Left.Name}={x.Right.Name}")));

                builder.AppendLine($");");
            }

            builder.Append("}");
            return builder.ToString();
        }
    }
    public record PinDescription(string Name);
    public record PinAssignmentDescription(PinDescription Left, PinDescription Right);
    public record PartDescription(string Name, List<PinAssignmentDescription> PinAssignments);
}
