using BenchmarkDotNet.Attributes;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace HDLTools.Benchmarks
{
    public class Not16Benchmarks
    {        
        Memory<byte> buffer = new byte[4] { 0b0000_1111, 0b0000_0000, 0, 0 };

        public Not16Benchmarks()
        {            
            
        }

        //[Benchmark]
        public void Not16()
        {
            ReadOnlySpan<byte> input = buffer.Slice(0, 2).Span;
            Span<byte> output = buffer.Slice(2, 2).Span;

            
            var a = Vector128.Create(input[0], input[1], 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
            var result = Sse2.And(a, a);
            result = Sse2.Xor(result, Vector128<byte>.AllBitsSet);
            
            output[0] = result.GetElement(0);
            output[1] = result.GetElement(1);
        }

    }
}
