﻿using BenchmarkDotNet.Attributes;
using HDLTools.Builtin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace HDLTools.Benchmarks
{
    public class NandBenchmarks
    {
        private Nand chip;
        private Pin1 pinA;
        private Pin1 pinB;
        private Pin1 pinOut;

        public NandBenchmarks()
        {
            chip = new Nand();
            pinA = chip.Pins.Single(x => x.Name == "a");
            pinB = chip.Pins.Single(x => x.Name == "b");
            pinOut = chip.Pins.Single(x => x.Name == "out");

            pinA.Init(0);
            pinB.Init(1);
        }

        //[Benchmark]
        public int NandGate()
        {                        
            chip.Evaluate();
            var result = pinOut.GetBit();
            return result;
        }

        //[Benchmark]
        public Vector128<byte> Avx2Nand()
        {
            byte byteA = 0b0000_0000;
            byte byteB = 0b1111_1111;
            byte byteC = 0b0101_0101;
            byte byteD = 0b1010_1010;

            var a = Vector128.Create(byteA, byteB, byteC, byteD, byteA, byteB, byteC, byteD, byteA, byteB, byteC, byteD, byteA, byteB, byteC, byteD);            
            var b = Vector128.Create(byteA, byteB, byteC, byteD, byteA, byteB, byteC, byteD, byteA, byteB, byteC, byteD, byteA, byteB, byteC, byteD);
            
            var result = Avx2.And(a, b);
            result = Avx2.Xor(result, Vector128<byte>.AllBitsSet);
            return result;
        }

    }
}
