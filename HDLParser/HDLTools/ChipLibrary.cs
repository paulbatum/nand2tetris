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
        protected Dictionary<string, ChipDescription> descriptions;

        public ChipLibrary()
        {
            descriptions = new Dictionary<string, ChipDescription>();

        }

        public void Register(ChipDescription description)
        {
            descriptions[description.Name] = description;
        }

        public ChipDescription GetDescription(string name)
        {
            return descriptions[name];
        }

        public void LoadAll(string hdlPath)
        {
            var hdlfiles = Directory.GetFiles(hdlPath, "*.hdl");
            foreach (var hdlfile in hdlfiles)
            {
                var content = File.ReadAllText(hdlfile);
                foreach (var chipDescription in HDLParser.ParseString(content))
                {
                    this.Register(chipDescription);
                }
            }
        }

        public Chip GetChip(string name)
        {
            if (descriptions.ContainsKey(name))
            {
                return new Chip(descriptions[name], this);
            }
            else
            {
                throw new KeyNotFoundException($"Couldn't find a description for part '{name}'.");
            }
        }
    }

    public class Chip1Library : ChipLibrary
    {
        public Chip1 GetChip1(string name, string fullyQualifiedParent)
        {
            if (name == "Nand")
                return new Nand(fullyQualifiedParent);

            if (name == "Identity4")
                return new Identity4(fullyQualifiedParent);

            if (name == "DFF")
                return new DelayFlipFlop(fullyQualifiedParent);

            if (descriptions.ContainsKey(name))
            {
                return new Chip1(descriptions[name], this, fullyQualifiedParent);
            }
            else
            {
                throw new KeyNotFoundException($"Couldn't find a description for part '{name}'.");
            }
        }
    }
}
