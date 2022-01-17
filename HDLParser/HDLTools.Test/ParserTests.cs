using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        [Theory]
        [InlineData("Not.hdl")]
        [InlineData("Mux.hdl")]
        public void RoundTrip(string filename)
        {            
            var input = File.ReadAllText(filename);
            input = HDLParser.RemoveComments(input);

            static string DoReplacements(string inputString) => inputString
                .Replace("    ", "\t")
                .Replace(" (", "(")
                .Replace(") ", ")")
                .Replace("  ", " ");

            List<ChipDescription> output = HDLParser.ParseString(input);
            var outputText = output[0].ToString();

            Assert.Equal(DoReplacements(input), DoReplacements(outputText));
        }

        [Fact]
        public void IgnoresSingleLineComments()
        {
            var hdl =
                @"CHIP Not {
                    IN in;
                    OUT out;

                    PARTS:
                    Nand (a=in, b=in, out=out);//Nand (a=in, b=in, out=out);
                    //Nand (a=in, b=in, out=out);
                }";

            ChipDescription desc = HDLParser.ParseString(hdl).Single();
            Assert.Single(desc.Parts);
        }

        [Fact]
        public void IgnoresMultiLineComments()
        {
            var hdl =
                @"CHIP Not {
                    IN in;
                    OUT out;

                    PARTS:
                    /*Nand (a=in, b=in, out=out);
                    Nand (a=in, b=in, out=out);*/
                    Nand (a=in, b=in, out=out);
                }";

            ChipDescription desc = HDLParser.ParseString(hdl).Single();
            Assert.Single(desc.Parts);
        }

        [Fact]
        public void HandlesMixOfComments()
        {
            var hdl =
                @" // this is a comment
                    CHIP Not {
                    IN in;
                    // IN in2;
                    OUT out;

                    PARTS:
                    /*Nand (a=in, b=in, out=out);
                    //Nand (a=in, b=in, out=out);
                    * Nand (a=in, b=in, out=out);
                    */
                    Na/*asdf*/nd (a=in, b=in, out=out);
                }";

            ChipDescription desc = HDLParser.ParseString(hdl).Single();
            Assert.Single(desc.Parts);
        }

    }
}
