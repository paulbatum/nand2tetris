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


        public Chip GetChip(string name)
        {
            if (name == "Nand")
                return new Nand();

            if (name == "Identity4")
                return new Identity4();

            if(descriptions.ContainsKey(name))
            {
                return new Chip(descriptions[name], this);
            }
            else
            {
                throw new KeyNotFoundException($"Couldn't find a description for part '{name}'.");
            }
        }
    }
}
