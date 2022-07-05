﻿using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDLTools.Benchmarks
{
    public class And16Benchmarks
    {
        private Chip chip;
        private Pin pinOut;

        public And16Benchmarks()
        {
            var library = new ChipLibrary();
            library.Register(HDLParser.ParseString(File.ReadAllText(@"hdl\Not.hdl")).Single());
            library.Register(HDLParser.ParseString(File.ReadAllText(@"hdl\And.hdl")).Single());

            var hdl = File.ReadAllText(@"hdl\And16.hdl");
            ChipDescription desc = HDLParser.ParseString(hdl).Single();

            chip = new Chip(desc, library);

            var pinA = chip.Pins.Single(x => x.Name == "a");
            var pinB = chip.Pins.Single(x => x.Name == "b");
            pinOut = chip.Pins.Single(x => x.Name == "out");

            pinA.SetBinaryStringValue("0011110011000011");
            pinB.SetBinaryStringValue("0000111111110000");
        }

        //[Benchmark]
        public ushort And16Chip()
        {
            chip.Evaluate();
            var result = pinOut.GetUShortValue();
            return result;
        }

    }
}
