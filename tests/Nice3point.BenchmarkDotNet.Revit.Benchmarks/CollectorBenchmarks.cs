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
// | Method                                   | Mean       | Error    | StdDev    | Median     | Gen0   | Gen1   | Allocated |
// |----------------------------------------- |-----------:|---------:|----------:|-----------:|-------:|-------:|----------:|
// | WhereElementIsNotElementTypeToElements   | 1,625.1 us | 29.09 us |  22.71 us | 1,620.0 us | 5.8594 | 3.9063 | 296.74 KB |
// | ElementIsElementTypeFilterToElements     | 1,713.2 us | 14.21 us |  13.29 us | 1,710.6 us | 5.8594 | 3.9063 | 296.94 KB |
// | WhereElementIsNotElementTypeToList       | 2,049.2 us | 17.77 us |  16.62 us | 2,047.6 us | 3.9063 |      - |  311.5 KB |
// | WhereElementIsNotElementTypeCastToList   | 1,053.4 us | 42.47 us | 123.89 us | 1,018.7 us | 0.9766 |      - |  94.68 KB |
// | WhereElementIsNotElementTypeOfTypeToList |   960.1 us | 13.22 us |  11.04 us |   958.1 us |      - |      - |  86.33 KB |

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class CollectorBenchmarks : RevitApiBenchmark
{
    private Document _document = null!;

    protected sealed override void OnGlobalSetup()
    {
        _document = Application.NewProjectDocument(UnitSystem.Metric);

        using var transaction = new Transaction(_document, "Seed model");
        transaction.Start();

        var level = Level.Create(_document, 0);
        for (var i = 0; i < 1000; i++)
        {
            Wall.Create(_document, Line.CreateBound(new XYZ(i, 0, 0), new XYZ(i + 1, 0, 0)), level.Id, false);
        }

        transaction.Commit();
    }

    protected sealed override void OnGlobalCleanup()
    {
        _document.Close(false);
    }

    [Benchmark]
    public IList<Element> WhereElementIsNotElementTypeToElements()
    {
        return new FilteredElementCollector(_document)
            .WhereElementIsNotElementType()
            .ToElements();
    }

    [Benchmark]
    public IList<Element> ElementIsElementTypeFilterToElements()
    {
        return new FilteredElementCollector(_document)
            .WherePasses(new ElementIsElementTypeFilter(true))
            .ToElements();
    }

    [Benchmark]
    public List<Element> WhereElementIsNotElementTypeToList()
    {
        return new FilteredElementCollector(_document)
            .WhereElementIsNotElementType()
            .ToList();
    }

    [Benchmark]
    public List<Wall> WhereElementIsNotElementTypeCastToList()
    {
        return new FilteredElementCollector(_document)
            .WhereElementIsNotElementType()
            .OfClass(typeof(Wall))
            .Cast<Wall>()
            .ToList();
    }

    [Benchmark]
    public List<Wall> WhereElementIsNotElementTypeOfTypeToList()
    {
        return new FilteredElementCollector(_document)
            .WhereElementIsNotElementType()
            .OfClass(typeof(Wall))
            .OfType<Wall>()
            .ToList();
    }
}
