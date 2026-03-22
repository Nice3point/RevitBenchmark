using BenchmarkDotNet.Attributes;

namespace Nice3point.BenchmarkDotNet.Revit.Tests;

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