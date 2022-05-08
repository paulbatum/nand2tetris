using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace HDLTools
{
    public class Chip2
    {
        public List<Pin2> Pins = new List<Pin2>();

        //public static Chip2 Create(ChipDescription description, ChipLibrary library)
        //{
        //    foreach (var childPart in description.Parts)
        //    {
        //        foreach (var p in childPart.PinAssignments)
        //        {
        //            p.
        //        }
        //    }

        //    return null;
        //}

        public void Evaluate()
        {
            
        }

        public static void ParseDescription(ChipDescription description)
        {

        }
    }

    public interface IChipComponent
    {
        ushort GetValue();
        void SetValue(ushort value);
        void Evaluate();
    }

    public readonly struct Nand2
    {
        public readonly IChipComponent A;
        public readonly IChipComponent B;
        public readonly IChipComponent Out;

        public void Evaluate()
        {
            ushort a = A.GetValue();
            ushort b = B.GetValue();

            ushort result = (ushort)~(a & b);
            Out.SetValue(result);
        }
    }

    public class Pin2
    {
        public string Name { get; }

        public Pin2(string name)
        {
            Name = name;
        }

        public void Init(int value)
        {
            throw new NotImplementedException();
        }

        public int GetBit()
        {
            throw new NotImplementedException();
        }
    }
}
