using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace HDLTools.Test
{
    public static class TestHelpers
    {
        public static void DumpChip(this ITestOutputHelper output, Chip chip, int cycle = 0)
        {
            var builder = new StringBuilder();
            chip.DumpTree(builder, cycle, "");
            output.WriteLine(builder.ToString());
        }

        public static void Init(this Pin pin, string binaryString)
        {
            pin.Init(Conversions.ConvertBinaryStringToIntArray(binaryString));
        }

        public static string GetValueString(this Pin pin, int cycle)
        {
            var val = pin.GetValue(cycle);
            return Conversions.ConvertIntArrayToBinaryString(val);
        }

        public static void LoadAll(this ChipLibrary library, string hdlPath)
        {
            var hdlfiles = Directory.GetFiles(hdlPath, "*.hdl");
            foreach (var hdlfile in hdlfiles)
            {
                var content = File.ReadAllText(hdlfile);
                foreach(var chipDescription in HDLParser.ParseString(content))
                {
                    library.Register(chipDescription);
                }
            }
        }
    }
}
