using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace HDLTools.Test.Chapter1
{
    public class NotTests
    {
        private class NotTestData : BinaryTestData
        {
            public NotTestData() : base ("Not.cmp")
            {}
        }

        [Theory]
        [ClassData(typeof(NotTestData))]
        public void BasicNot(int inValue, int outValue)
        {
            var library = new ChipLibrary();
            var notHDL = File.ReadAllText(@"hdl\Not.hdl");
            ChipDescription notDescription = HDLParser.ParseString(notHDL).Single();

            Chip notChip = new Chip(notDescription, library);

            var cycle = 0;
            var inputPin = notChip.Pins.Single(x => x.Name == "in");
            var outputPin = notChip.Pins.Single(x => x.Name == "out");
            inputPin.Init(inValue);

            notChip.Simulate(cycle);

            Assert.Equal(outValue, outputPin.GetBit(cycle));
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(1, 0)]
        public void WrappedNot(int inValue, int outValue)
        {
            var library = new ChipLibrary();
            var notHDL = File.ReadAllText(@"hdl\Not.hdl");
            ChipDescription notDescription = HDLParser.ParseString(notHDL).Single();
            library.Register(notDescription);

            var wrappedNotHDL =
                @"CHIP WrappedNot {
                    IN in;
                    OUT out;

                    PARTS:
                    Not (in=in, out=out);
                }";

            ChipDescription wrappedNotDescription = HDLParser.ParseString(wrappedNotHDL).Single();

            Chip wrappedNotChip = new Chip(wrappedNotDescription, library);

            var cycle = 0;
            var inputPin = wrappedNotChip.Pins.Single(x => x.Name == "in");
            var outputPin = wrappedNotChip.Pins.Single(x => x.Name == "out");
            inputPin.Init(inValue);

            wrappedNotChip.Simulate(cycle);

            Assert.Equal(outValue, outputPin.GetBit(cycle));
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 1)]
        public void DoubleNot(int inValue, int outValue)
        {
            var library = new ChipLibrary();
            var notHDL = File.ReadAllText(@"hdl\Not.hdl");
            ChipDescription notDescription = HDLParser.ParseString(notHDL).Single();
            library.Register(notDescription);

            var doubleNotHDL =
                @"CHIP NotNot {
                    IN in;
                    OUT out;

                    PARTS:
                    Not (in=in, out=notin);
                    Not (in=notin, out=out);
                }";

            ChipDescription doubleNotDescription = HDLParser.ParseString(doubleNotHDL).Single();

            Chip doubleNotChip = new Chip(doubleNotDescription, library);

            var cycle = 0;
            var inputPin = doubleNotChip.Pins.Single(x => x.Name == "in");
            var outputPin = doubleNotChip.Pins.Single(x => x.Name == "out");
            inputPin.Init(inValue);

            doubleNotChip.Simulate(cycle);

            Assert.Equal(outValue, outputPin.GetBit(cycle));
        }
    }
}
