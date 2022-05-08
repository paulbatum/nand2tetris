using BenchmarkDotNet.Running;
using HDLTools.Benchmarks;

var summary = BenchmarkRunner.Run(typeof(Program).Assembly);
//var b = new NandBenchmarks();
//b.Avx2Nand();
