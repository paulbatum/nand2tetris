using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDLTools
{
    public class Conversions
    {
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
