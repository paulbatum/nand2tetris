using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace HDLTools.Test
{
    public class ParserTests
    {
        [Fact]
        public void ParsesNot()
        {
            var input = File.ReadAllText("Not.hdl");
            List<ChipDescription> output = HDLParser.ParseString(input);

            var not = Assert.Single(output);
            Assert.Equal("Not", not.Name);

            var inputPin = Assert.Single(not.InputPins);
            Assert.Equal("in", inputPin.Name);

            var outputPin = Assert.Single(not.OutputPins);
            Assert.Equal("out", outputPin.Name);

            var part = Assert.Single(not.Parts);
            Assert.Equal("Nand", part.Name);

            Assert.Equal(3, part.PinAssignments.Count);

            var assignment0 = part.PinAssignments[0];
            Assert.Equal("a", assignment0.Left.Name);
            Assert.Equal("in", assignment0.Right.Name);

            var assignment1 = part.PinAssignments[1];
            Assert.Equal("b", assignment1.Left.Name);
            Assert.Equal("in", assignment1.Right.Name);

            var assignment2 = part.PinAssignments[2];
            Assert.Equal("out", assignment2.Left.Name);
            Assert.Equal("out", assignment2.Right.Name);
        }

        [Fact]
        public void ParsesFileWithTwoChips()
        {
            var input = File.ReadAllText("NotAndMux.hdl");
            List<ChipDescription> output = HDLParser.ParseString(input);

            Assert.Equal(2, output.Count);            
        }
    }
}
