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
            pin.Init(ConvertBinaryStringToIntArray(binaryString));
        }

        public static string GetValueString(this Pin pin, int cycle)
        {
            var val = pin.GetValue(cycle);
            return ConvertIntArrayToBinaryString(val);
        }

        public static int[] ConvertBinaryStringToIntArray(string binaryString)
        {
            return binaryString
                .ToCharArray()
                .Select(c => int.Parse(c.ToString()))
                .ToArray();
        }

        public static string ConvertIntArrayToBinaryString(int[] values)
        {
            return string.Join("", values.Select(x => x.ToString()));
        }
    }
}
