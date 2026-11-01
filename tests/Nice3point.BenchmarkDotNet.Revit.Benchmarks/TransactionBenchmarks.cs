using System.Globalization;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.dotTrace;

namespace Nice3point.BenchmarkDotNet.Revit.Benchmarks;

// BenchmarkDotNet v0.15.8, Windows 11 (10.0.28000.2704/26H1/2026Update)
// AMD Ryzen 9 9950X3D 4.30GHz, 1 CPU, 32 logical and 16 physical cores
// .NET SDK 10.0.112
//   [Host]     : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v4
//   Job-YLFTCN : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v4
//
// BuildConfiguration=Release.R27  Toolchain=RevitUi
//
// | Method                          | Mean      | Error    | StdDev   | Allocated  |
// |-------------------------------- |----------:|---------:|---------:|-----------:|
// | SetCommentsInOneTransaction     |  48.24 ms | 0.951 ms | 0.934 ms |   72.34 KB |
// | SetCommentsInTransactionPerWall | 332.45 ms | 6.470 ms | 7.451 ms | 1678.73 KB |

[DotTraceDiagnoser]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class TransactionBenchmarks : RevitApiUiBenchmark
{
    private Document _document = null!;
    private List<Wall> _walls = null!;
    private int _revision;

    protected sealed override void OnGlobalSetup()
    {
        var modelPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.rvt");
        var document = UiApplication.Application.NewProjectDocument(UnitSystem.Metric);

        using (var transaction = new Transaction(document, "Seed model"))
        {
            transaction.Start();

            var level = Level.Create(document, 0);
            for (var i = 0; i < 100; i++)
            {
                Wall.Create(document, Line.CreateBound(new XYZ(i, 0, 0), new XYZ(i + 1, 0, 0)), level.Id, false);
            }

            transaction.Commit();
        }

        document.SaveAs(modelPath);
        document.Close(false);

        _document = UiApplication.OpenAndActivateDocument(modelPath).Document;
        _walls = new FilteredElementCollector(_document)
            .OfClass(typeof(Wall))
            .Cast<Wall>()
            .ToList();
    }

    protected sealed override void OnGlobalCleanup()
    {
        _document.Save();
    }

    [Benchmark]
    public void SetCommentsInOneTransaction()
    {
        var comment = CreateComment();

        using var transaction = new Transaction(_document, "Set comments");
        transaction.Start();

        foreach (var wall in _walls)
        {
            wall.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).Set(comment);
        }

        transaction.Commit();
    }

    [Benchmark]
    public void SetCommentsInTransactionPerWall()
    {
        var comment = CreateComment();

        foreach (var wall in _walls)
        {
            using var transaction = new Transaction(_document, "Set comment");
            transaction.Start();
            wall.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).Set(comment);
            transaction.Commit();
        }
    }

    private string CreateComment()
    {
        return (++_revision).ToString(CultureInfo.InvariantCulture);
    }
}
