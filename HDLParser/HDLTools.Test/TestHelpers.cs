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
    }
}
