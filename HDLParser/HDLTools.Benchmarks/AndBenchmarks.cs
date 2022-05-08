using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace HDLTools.Benchmarks
{    
    public class AndBenchmarks
    {
        int[] inputA = new int[4] { 0, 1, 0, 1 };
        int[] inputB = new int[4] { 0, 1, 1, 0 };
        int[] output = new int[4] ;


        public AndBenchmarks()
        {

        }

        [Benchmark]
        public void AcceleratedAnd()
        {
            var vectorA = Vector128.Create(inputA[0], inputA[1], inputA[2], inputA[3]);
            var vectorB = Vector128.Create(inputB[0], inputB[1], inputB[2], inputB[3]);

            var nandResult = Sse2.Xor(
                Sse2.And(vectorA, vectorB),
                Vector128<int>.AllBitsSet
            );

            var result = Sse2.Xor(
                Sse2.And(nandResult, nandResult),
                Vector128<int>.AllBitsSet
            );

            output[0] = result.GetElement(0);
            output[1] = result.GetElement(1);
            output[2] = result.GetElement(2);
            output[3] = result.GetElement(3);            
        }

        // this was slower
        [Benchmark]
        public unsafe void AcceleratedAndUnsafe()
        { 
            fixed (int* aPtr = inputA)
            fixed (int* bPtr = inputB)
            fixed (int* outPtr = output)
            {
                var vectorA = Sse3.LoadDquVector128(aPtr);
                var vectorB = Sse3.LoadDquVector128(bPtr);

                var nandResult = Sse2.Xor(
                    Sse2.And(vectorA, vectorB),
                    Vector128<int>.AllBitsSet
                );

                Sse2.Store(outPtr, Sse2.Xor(
                    Sse2.And(nandResult, nandResult),
                    Vector128<int>.AllBitsSet
                ));                
            }
        }

    }
}
