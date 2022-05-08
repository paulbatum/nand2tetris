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
        public static void DumpChip(this ITestOutputHelper output, Chip chip)
        {
            var builder = new StringBuilder();
            chip.DumpTree(builder, "");
            output.WriteLine(builder.ToString());
        }

        public static void Init(this Pin pin, string binaryString)
        {
            pin.Init(Conversions.ConvertBinaryStringToIntArray(binaryString));
        }

        public static string GetValueString(this Pin pin)
        {
            var val = pin.GetValue();
            return Conversions.ConvertIntArrayToBinaryString(val);
        }

    }
}
