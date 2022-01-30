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
        [InlineData("Mux16.hdl")]
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

        [Fact]
        public void HandlesVariableWidthInputOutputPins()
        {
            var hdl =
                @"CHIP Test {
                    IN a[1], b[2], c[3], d[4], sel;
                    OUT out[16];

                    PARTS:
                    Mux16(a=a, b=b, sel=sel, out=out);
                }";

            ChipDescription desc = HDLParser.ParseString(hdl).Single();

            Assert.Equal(5, desc.InputPins.Count);
            
            Assert.Equal(1, desc.InputPins[0].Width);            
            Assert.Equal(2, desc.InputPins[1].Width);
            Assert.Equal(3, desc.InputPins[2].Width);
            Assert.Equal(4, desc.InputPins[3].Width);
            Assert.Equal(1, desc.InputPins[4].Width);

            Assert.Single(desc.OutputPins);
            Assert.Equal(16, desc.OutputPins[0].Width);
        }

        [Fact]
        public void HandlesPinIndexingAssignments()
        {
            var hdl =
                @"CHIP Test {
                    IN a[16], b[16], sel;
                    OUT out[16];

                    PARTS:
                    Mux (a=b[7], b=a[7], sel=sel, out=out[7]);
                }";

            ChipDescription desc = HDLParser.ParseString(hdl).Single();

            Assert.Single(desc.Parts);

            var mux = desc.Parts[0];
            Assert.Equal(4, mux.PinAssignments.Count);

            
            var assignment0 = mux.PinAssignments[0]; // a=b[7]
            Assert.Equal("a", assignment0.Left.Name);
            Assert.False(assignment0.Left.IsIndexed);
            Assert.Equal("b", assignment0.Right.Name);
            Assert.Equal(7, assignment0.Right.Index);

            var assignment1 = mux.PinAssignments[1]; // b=a[7]
            Assert.Equal("b", assignment1.Left.Name);
            Assert.False(assignment1.Left.IsIndexed);
            Assert.Equal("a", assignment1.Right.Name);
            Assert.Equal(7, assignment1.Right.Index);

            var assignment2 = mux.PinAssignments[2]; // sel=sel
            Assert.Equal("sel", assignment2.Left.Name);
            Assert.False(assignment2.Left.IsIndexed);
            Assert.Equal("sel", assignment2.Right.Name);
            Assert.False(assignment2.Right.IsIndexed);

            var assignment3 = mux.PinAssignments[3]; // out=out[7]
            Assert.Equal("out", assignment3.Left.Name);
            Assert.False(assignment3.Left.IsIndexed);
            Assert.Equal("out", assignment3.Right.Name);
            Assert.Equal(7, assignment3.Right.Index);
        }
    }
}
