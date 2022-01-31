﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace HDLTools.Test.Chapter2
{
    public class AluTests
    {
        private class AluNoStatData : BinaryTestData
        {
            public AluNoStatData() : base("ALU-nostat.cmp")
            { }
        }

        private class AluTestData : BinaryTestData
        {
            public AluTestData() : base("ALU.cmp")
            { }
        }

        private readonly ITestOutputHelper testOutput;
        public AluTests(ITestOutputHelper output)
        {
            this.testOutput = output;
        }

        [Theory]
        [ClassData(typeof(AluNoStatData))]
        public void AluNoStatCompareFile(int[] x, int[] y, int zx, int nx, int zy, int ny, int f, int no, int[] outValue)
        {
            var library = new ChipLibrary();
            library.LoadAll("hdl");

            var hdl = File.ReadAllText(@"hdl\ALU.hdl");
            ChipDescription desc = HDLParser.ParseString(hdl).Single();

            Chip chip = new Chip(desc, library);

            var cycle = 0;

            var singleInputPins = new[] { "zx", "nx", "zy", "ny", "f", "no" }    
                .Select(x => chip.Pins.Single(p => p.Name == x.ToString()))
                .ToArray();

            var pinX = chip.Pins.Single(x => x.Name == "x");
            var pinY = chip.Pins.Single(x => x.Name == "y");
            var pinOut = chip.Pins.Single(x => x.Name == "out");

            var singleInputValues = new[] { zx, nx, zy, ny, f, no };

            for (int index = 0; index < singleInputValues.Length; index++)
            {
                singleInputPins[index].Init(singleInputValues[index]);
            }

            pinX.Init(x);
            pinY.Init(y);

            chip.Simulate(cycle);

            Assert.Equal(outValue, pinOut.GetValue(cycle));
        }

        [Theory]
        [ClassData(typeof(AluTestData))]
        public void AluCompareFile(int[] x, int[] y, int zx, int nx, int zy, int ny, int f, int no, int[] outValue, int zr, int ng)
        {
            var library = new ChipLibrary();
            library.LoadAll("hdl");

            var hdl = File.ReadAllText(@"hdl\ALU.hdl");
            ChipDescription desc = HDLParser.ParseString(hdl).Single();

            Chip chip = new Chip(desc, library);

            var cycle = 0;

            var singleInputPins = new[] { "zx", "nx", "zy", "ny", "f", "no" }
                .Select(x => chip.Pins.Single(p => p.Name == x.ToString()))
                .ToArray();

            var pinX = chip.Pins.Single(x => x.Name == "x");
            var pinY = chip.Pins.Single(x => x.Name == "y");
            var pinOut = chip.Pins.Single(x => x.Name == "out");
            var pinZr = chip.Pins.Single(x => x.Name == "zr");
            var pinNg = chip.Pins.Single(x => x.Name == "ng");

            var singleInputValues = new[] { zx, nx, zy, ny, f, no };

            for (int index = 0; index < singleInputValues.Length; index++)
            {
                singleInputPins[index].Init(singleInputValues[index]);
            }

            pinX.Init(x);
            pinY.Init(y);

            chip.Simulate(cycle);

            Assert.Equal(outValue, pinOut.GetValue(cycle));
            Assert.Equal(zr, pinZr.GetBit(cycle));
            Assert.Equal(ng, pinNg.GetBit(cycle));
        }
    }
}
