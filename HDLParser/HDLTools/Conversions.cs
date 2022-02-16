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

        public static string ConvertDecimalIntToBinaryString(int value, int length)
        {
            var binaryString = Convert.ToString((short) value, 2);
            return binaryString.PadLeft(length, '0');
        }

        public static int[] ConvertDecimalIntToIntArray(int value, int length)
        {
            var binaryString = ConvertDecimalIntToBinaryString(value, length);
            return ConvertBinaryStringToIntArray(binaryString);
        }

        public static string ConvertIntArrayToDecimalString(int[] values)
        {
            var binaryString = ConvertIntArrayToBinaryString(values);
            return Convert.ToInt32(binaryString, 2).ToString();
        }
    }
}
