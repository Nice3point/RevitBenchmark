using BenchmarkDotNet.Attributes;

namespace Nice3point.BenchmarkDotNet.Revit.Tests;

[MemoryDiagnoser]
public class RevitApplicationBenchmarks : RevitApiBenchmark
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