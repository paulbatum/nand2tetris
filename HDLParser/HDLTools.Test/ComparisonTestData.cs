using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDLTools.Test
{
    internal abstract class ComparisonTestData : IEnumerable<object[]>
    {
        private string comparisonFileName;

        public ComparisonTestData(string comparisonFileName)
        {
            this.comparisonFileName = comparisonFileName;
        }

        public IEnumerator<object[]> GetEnumerator()
        {
            var lines = File.ReadAllLines(comparisonFileName)
                .Skip(1);

            foreach (var line in lines)
            {
                var inputStrings = line
                    .Split('|')
                    .Select(s => s.Trim())
                    .Where(s => s.Length > 0)
                    .ToArray();

                yield return ParseInputStrings(inputStrings);
            }
        }

        protected abstract object[] ParseInputStrings(string[] inputStrings);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    internal class BinaryTestData : ComparisonTestData
    {
        public BinaryTestData(string comparisonFileName) : base(comparisonFileName)
        {
        }

        protected override object[] ParseInputStrings(string[] inputStrings)
        {
            List<object> outputs = new List<object>();

            foreach (var inputString in inputStrings)
            {
                if(inputString.Length > 1)
                {
                    outputs.Add(inputString
                        .ToCharArray()
                        .Select(c => int.Parse(c.ToString()))
                        .ToArray()
                    );
                }
                else
                {
                    outputs.Add(int.Parse(inputString));
                }
            }

            return outputs.ToArray();
        }
    }
}
