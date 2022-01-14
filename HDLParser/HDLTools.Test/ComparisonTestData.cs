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
            return File.ReadAllLines(comparisonFileName)
                .Skip(1)
                .Select(x => x.Split("|").Select(s => s.Trim()).Where(s => s.Length > 0).Select(s => (object)int.Parse(s)).ToArray())
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
