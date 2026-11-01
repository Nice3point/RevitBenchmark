using BenchmarkDotNet.Attributes;

namespace Nice3point.BenchmarkDotNet.Revit.Benchmarks;

// BenchmarkDotNet v0.15.8, Windows 11 (10.0.28000.2704/26H1/2026Update)
// AMD Ryzen 9 9950X3D 4.30GHz, 1 CPU, 32 logical and 16 physical cores
// .NET SDK 10.0.112
//   [Host]     : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v4
//   Job-AAUCHH : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v4
//
// BuildConfiguration=Release.R27
//
// | Method       | Mean     | Error    | StdDev   | Gen0   | Allocated |
// |------------- |---------:|---------:|---------:|-------:|----------:|
// | NewXyz       | 12.12 ns | 0.259 ns | 0.243 ns | 0.0013 |      64 B |
// | CreateNewXyz | 11.76 ns | 0.128 ns | 0.113 ns | 0.0013 |      64 B |

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class ApplicationBenchmarks : RevitApiBenchmark
{
    [Benchmark]
    public XYZ NewXyz()
    {
        return new XYZ(3, 4, 5);
    }

    [Benchmark]
    public XYZ CreateNewXyz()
    {
        return Application.Create.NewXYZ(3, 4, 5);
    }
}
