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
            builder.AppendLine(string.Join(", ", InputPins.Select(x => x.ToString())) + ";");

            builder.Append("\tOUT ");
            builder.AppendLine(string.Join(", ", OutputPins.Select(x => x.ToString())) + ";");
            builder.AppendLine();

            builder.AppendLine("\tPARTS:");            
            foreach (var part in Parts)
            {
                builder.Append($"\t{part.Name} (");
                builder.Append(string.Join(", ", part.PinAssignments.Select(x => x.ToString())));

                builder.AppendLine($");");
            }

            builder.Append("}");
            return builder.ToString();
        }
    }
    public record PinDescription(string Name, int Width = 1)
    {
        public override string ToString()
        {
            return Width == 1 ? Name : $"{Name}[{Width}]";
        }
    }

    public record PinReference(string Name, bool IsIndexed, int StartIndex, int EndIndex)
    {
        public override string ToString()
        {
            if (IsIndexed)
            {
                return StartIndex == EndIndex ? $"{Name}[{StartIndex}]" : $"{Name}[{StartIndex}..{EndIndex}]";
            }

            return Name;
        }
    }

    public record PinAssignmentDescription(PinReference Left, PinReference Right)
    {
        public override string ToString()
        {
            return $"{Left}={Right}";
        }
    }

    public record PartDescription(string Name, List<PinAssignmentDescription> PinAssignments);
}
