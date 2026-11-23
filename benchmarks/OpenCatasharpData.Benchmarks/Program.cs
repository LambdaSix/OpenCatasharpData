using BenchmarkDotNet.Running;
using OpenCatasharpData.Benchmarks;

BenchmarkSwitcher.FromAssembly(typeof(RecordParsingBenchmarks).Assembly).Run(args);
