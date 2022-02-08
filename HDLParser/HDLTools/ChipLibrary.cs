using HDLTools.Builtin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDLTools
{
    public class ChipLibrary
    {
        private Dictionary<string, ChipDescription> descriptions;

        public ChipLibrary()
        {
            descriptions = new Dictionary<string, ChipDescription>();

        }

        public void Register(ChipDescription description)
        {
            descriptions[description.Name] = description;
        }


        public Chip GetChip(string name, string fullyQualifiedParent)
        {
            if (name == "Nand")
                return new Nand(fullyQualifiedParent);

            if (name == "Identity4")
                return new Identity4(fullyQualifiedParent);

            if (name == "DFF")
                return new DelayFlipFlop(fullyQualifiedParent);

            if(descriptions.ContainsKey(name))
            {
                return new Chip(descriptions[name], this, fullyQualifiedParent);
            }
            else
            {
                throw new KeyNotFoundException($"Couldn't find a description for part '{name}'.");
            }
        }
    }
}
