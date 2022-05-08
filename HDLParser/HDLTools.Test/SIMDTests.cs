using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace HDLTools.Test
{
    public class SIMDTests
    {
        private readonly ITestOutputHelper testOutput;
        public SIMDTests(ITestOutputHelper output)
        {
            this.testOutput = output;
        }

        [Theory]
        [InlineData((ushort)0b0000_0000_0000_0000, (ushort)0b0000_0000_0000_0000, (ushort)0b1111_1111_1111_1111)]
        [InlineData((ushort)0b0000_0000_0000_0001, (ushort)0b0000_0000_0000_0001, (ushort)0b1111_1111_1111_1110)]
        [InlineData((ushort)0b1111_1111_1111_1111, (ushort)0b1111_1111_1111_1111, (ushort)0b0000_0000_0000_0000)]
        [InlineData((ushort)0b1111_1111_0000_1111, (ushort)0b1111_0000_1111_1111, (ushort)0b0000_1111_1111_0000)]
        public void BitwiseOperatorsNand(ushort a, ushort b, ushort outValue)
        {
            ushort result = (ushort) ~(a & b);
            Assert.Equal(outValue, result);
        }

        [Theory]
        [InlineData(0, 0, 1)]
        [InlineData(1, 1, 0)]
        [InlineData(0, 1, 1)]
        [InlineData(1, 0, 1)]
        public void AcceleratedNandScalar(int a, int b, int outValue)
        {
            var vectorA = Vector128.CreateScalar(a);
            var vectorB = Vector128.CreateScalar(b);
            
            // I can't find a SIMD NAND, so we'll always use a XOR(AND(a,b),1)

            var result = Sse2.Xor(
                Sse2.And(vectorA, vectorB),
                Vector128.CreateScalar(1)
            ).ToScalar();

            Assert.Equal(outValue, result);
        }

        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(1, 1, 1)]
        [InlineData(0, 1, 0)]
        [InlineData(1, 0, 0)]
        public void AcceleratedAndScalar(int a, int b, int outValue)
        {
            var vectorA = Vector128.CreateScalar(a);
            var vectorB = Vector128.CreateScalar(b);

            // we have to do this with NAND operations

            // first NAND a and b           
            var nandResult = Sse2.Xor(
                Sse2.And(vectorA, vectorB),
                Vector128.CreateScalar(1)
            );

            // then NOT the result by NANDing it with itself
            var result = Sse2.Xor(
                Sse2.And(nandResult, nandResult),
                Vector128.CreateScalar(1)
            ).ToScalar();

            Assert.Equal(outValue, result);
        }

        [Fact]
        public void AcceleratedAnd()
        {
            int[] inputA = new int[4] { 0, 1, 0, 1 };
            int[] inputB = new int[4] { 0, 1, 1, 0 };
            int[] expected = new int[4] { 0, 1, 0, 0 };

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

            for (int i = 0; i < expected.Length; i++)
            {
                Assert.Equal(expected[i], result.GetElement(i));
            }
        }

        [Fact]
        public unsafe void AcceleratedAndUnsafe()
        {
            int[] inputA = new int[4] { 0, 1, 0, 1 };
            int[] inputB = new int[4] { 0, 1, 1, 0 };            
            int[] expected = new int[4] { 0, 1, 0, 0 };

            fixed (int* aPtr = inputA)
            fixed (int* bPtr = inputB)
            {
                var vectorA = Sse3.LoadDquVector128(aPtr);
                var vectorB = Sse3.LoadDquVector128(bPtr);

                var nandResult = Sse2.Xor(
                    Sse2.And(vectorA, vectorB),
                    Vector128<int>.AllBitsSet
                );

                var result = Sse2.Xor(
                    Sse2.And(nandResult, nandResult),
                    Vector128<int>.AllBitsSet
                );
                
                for (int i = 0; i < expected.Length; i++)
                {
                    Assert.Equal(expected[i], result.GetElement(i));
                }
            }            
        }

        [Fact]
        public void AcceleratedMuxScalar()
        {
            short inputA    = 0b00001111;
            short inputB    = 0b00110011;
            short sel       = 0b01010101;
            short expected  = 0b00011011;

            Vector128<short> Nand(Vector128<short> left, Vector128<short> right)
            {
                return Sse2.Xor(
                    Sse2.And(left, right),
                    Vector128<short>.AllBitsSet
                );
            }

            Vector128<short> Not(Vector128<short> vector)
            {
                return Nand(vector, vector);
            }

            Vector128<short> And(Vector128<short> left, Vector128<short> right)
            {
                return Not(Nand(left, right));
            }

            Vector128<short> Or(Vector128<short> left, Vector128<short> right)
            {
                return Not(
                    And(Not(left), Not(right))
                );
            }

            Vector128<short> Mux(Vector128<short> a, Vector128<short> b, Vector128<short> sel)
            {
                return Or(
                    And(a, Not(sel)),
                    And(b, sel)
                );
            }

            var vectorA = Vector128.CreateScalar(inputA);
            var vectorB = Vector128.CreateScalar(inputB);
            var vectorSel = Vector128.CreateScalar(sel);

            short result = Mux(vectorA, vectorB, vectorSel).ToScalar();

            Assert.Equal(expected, result);

        }
    }
}
