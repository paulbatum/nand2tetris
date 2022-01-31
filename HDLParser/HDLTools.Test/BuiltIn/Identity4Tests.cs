using HDLTools.Builtin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace HDLTools.Test.BuiltIn
{
    public class Identity4Tests
    {
        [Theory]
        [InlineData("0000", "0000")]
        [InlineData("1111", "1111")]
        [InlineData("1010", "1010")]
        [InlineData("0101", "0101")]
        public void BasicIdentity4(string inValue, string outValue)
        {
            var chip = new Identity4();


            var cycle = 0;
            var pinIn = chip.Pins.Single(x => x.Name == "in");            
            var pinOut = chip.Pins.Single(x => x.Name == "out");
            
            pinIn.Init(inValue);            
            chip.Simulate(cycle);

            Assert.Equal(outValue, pinOut.GetValueString(cycle));
        }
    }
}
