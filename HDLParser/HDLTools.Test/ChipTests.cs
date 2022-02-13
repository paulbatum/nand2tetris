using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace HDLTools.Test
{
    public class ChipTests
    {
        private readonly ITestOutputHelper testOutput;
        public ChipTests(ITestOutputHelper output)
        {
            this.testOutput = output;
        }

        [Fact]
        public void SingleWidthAssignment()
        {
            var hdl =
                @"CHIP Not {
                    IN in;
                    OUT out;

                    PARTS:
                    Nand (a=in, b=in, out=out);
                }";

            ChipDescription description = HDLParser.ParseString(hdl).Single();
            var library = new ChipLibrary();            
            var notChip = new Chip(description, library);

            var inPin = notChip.Pins.Single(x => x.Name == "in");
            var outPin = notChip.Pins.Single(x => x.Name == "out");

            inPin.Init(0);
            notChip.Evaluate();

            Assert.Equal(1, outPin.GetBit());
        }

        [Theory]
        [InlineData("00", 1)]
        [InlineData("10", 1)]
        [InlineData("01", 1)]
        [InlineData("11", 0)]
        public void IndexedInput(string inString, int outValue)
        {
            var hdl =
                @"CHIP Nand2Bit {
                    IN in[2];
                    OUT out;

                    PARTS:
                    Nand (a=in[0], b=in[1], out=out);
                }";

            ChipDescription description = HDLParser.ParseString(hdl).Single();
            var library = new ChipLibrary();
            var chip = new Chip(description, library);

            var inPin = chip.Pins.Single(x => x.Name == "in");
            var outPin = chip.Pins.Single(x => x.Name == "out");            

            inPin.Init(inString);
            chip.Evaluate();

            Assert.Equal(outValue, outPin.GetBit());
        }

        [Theory]
        [InlineData(0, 0, 0, 0, "11")]
        [InlineData(0, 1, 0, 1, "11")]
        [InlineData(1, 0, 1, 0, "11")]
        [InlineData(1, 1, 1, 0, "01")]
        [InlineData(1, 0, 1, 1, "10")]
        [InlineData(1, 1, 1, 1, "00")]
        public void IndexedOutput(int a, int b, int c, int d, string outValue)
        {
            // remember that indexing is right to left
            var hdl =
                @"CHIP Nand2Bit {
                    IN a,b,c,d;
                    OUT out[2];

                    PARTS:
                    Nand (a=a, b=b, out=out[1]);
                    Nand (a=c, b=d, out=out[0]);
                }";

            ChipDescription description = HDLParser.ParseString(hdl).Single();
            var library = new ChipLibrary();
            var chip = new Chip(description, library);            
            
            var outPin = chip.Pins.Single(x => x.Name == "out");

            var pinA = chip.Pins.Single(x => x.Name == "a");
            var pinB = chip.Pins.Single(x => x.Name == "b");
            var pinC = chip.Pins.Single(x => x.Name == "c");
            var pinD = chip.Pins.Single(x => x.Name == "d");

            pinA.Init(a);
            pinB.Init(b);
            pinC.Init(c);   
            pinD.Init(d);

            chip.Evaluate();
            Assert.Equal(outValue, outPin.GetValueString());
        }

        [Theory]
        [InlineData("00000000", "0000")]        
        [InlineData("11110000", "0000")]
        [InlineData("00000001", "0001")]
        [InlineData("00001011", "1011")]
        public void RangeIndexedInput(string a, string outValue)
        {
            var test =
                @"CHIP Test {
                    IN a[8];
                    OUT out[4];

                    PARTS:
                    Identity4 (in=a[0..3], out=out);
                }";

            var library = new ChipLibrary();
            var chip = new Chip(HDLParser.ParseString(test).Single(), library);

            var pinA = chip.Pins.Single(x => x.Name == "a");
            var outPin = chip.Pins.Single(x => x.Name == "out");

            pinA.Init(a);

            chip.Evaluate();
            Assert.Equal(outValue, outPin.GetValueString());
        }

        [Theory]
        [InlineData("00000000")]
        [InlineData("11111111")]
        [InlineData("10101010")]
        [InlineData("01101110")]
        public void RangeIndexedInputAndOutput(string input)
        {
            var identity8 =
                @"CHIP Identity8 {
                    IN a[8];
                    OUT out[8];

                    PARTS:
                    Identity4 (in=a[0..3], out=out[0..3]);
                    Identity4 (in=a[4..7], out=out[4..7]);
                }";

            var library = new ChipLibrary();
            var chip = new Chip(HDLParser.ParseString(identity8).Single(), library);
            

            var pinA = chip.Pins.Single(x => x.Name == "a");            
            var outPin = chip.Pins.Single(x => x.Name == "out");            

            pinA.Init(input);

            chip.Evaluate();
            Assert.Equal(input, outPin.GetValueString());
        }
        
        [Theory]
        [InlineData("0000", "00", "00")]
        [InlineData("1001", "01", "10")]
        public void OutputLeftIndexer(string a, string low2, string high2)
        {
            var hdl =
                @"CHIP Test {
                    IN a[4];
                    OUT low2[2], high2[2];

                    PARTS:
                    Identity4 (in=a, out[0..1]=low2, out[2..3]=high2);                    
                }";

            var library = new ChipLibrary();
            var chip = new Chip(HDLParser.ParseString(hdl).Single(), library);

            var pinA = chip.Pins.Single(x => x.Name == "a");
            var pinLow2 = chip.Pins.Single(x => x.Name == "low2");
            var pinHigh2 = chip.Pins.Single(x => x.Name == "high2");

            pinA.Init(a);

            chip.Evaluate();
            Assert.Equal(low2, pinLow2.GetValueString());
            Assert.Equal(high2, pinHigh2.GetValueString());
        }

        [Theory]
        [InlineData("0000", 1)]
        [InlineData("0010", 1)]
        [InlineData("0001", 1)]
        [InlineData("0011", 0)]
        [InlineData("1101", 1)]
        public void OutputLeftInternal(string a, int outValue)
        {
            var hdl =
                @"CHIP Test{
                    IN a[4];
                    OUT out;

                    PARTS:
                    Identity4 (in=a, out[0..1]=temp);
                    Nand (a=temp[0], b=temp[1], out=out);
                }";

            var library = new ChipLibrary();
            var chip = new Chip(HDLParser.ParseString(hdl).Single(), library);

            var pinA = chip.Pins.Single(x => x.Name == "a");
            var outPin = chip.Pins.Single(x => x.Name == "out");

            pinA.Init(a);

            chip.Evaluate();
            Assert.Equal(outValue, outPin.GetBit());
        }

        [Fact]
        public void EvaluateDoesNotUseCachedValues()
        {
            var hdl =
                @"CHIP And {
                    IN a, b;
                    OUT out;

                    PARTS:
                    Nand(a=a, b=b, out=nandab);
                    Not(in=nandab, out=out);
                }";

            ChipDescription description = HDLParser.ParseString(hdl).Single();
            var library = new ChipLibrary();
            library.Register(HDLParser.ParseString(File.ReadAllText(@"hdl\Not.hdl")).Single());
            var chip = new Chip(description, library);

            var pinA = chip.Pins.Single(x => x.Name == "a");
            var pinB = chip.Pins.Single(x => x.Name == "b");
            var pinOut = chip.Pins.Single(x => x.Name == "out");

            pinA.Init(1);
            pinB.Init(1);
            chip.Evaluate();

            Assert.Equal(1, pinOut.GetBit());

            pinB.Init(0);
            chip.Evaluate();

            Assert.Equal(0, pinOut.GetBit());

            pinB.Init(1);
            chip.Evaluate();

            Assert.Equal(1, pinOut.GetBit());
        }
    }
}
