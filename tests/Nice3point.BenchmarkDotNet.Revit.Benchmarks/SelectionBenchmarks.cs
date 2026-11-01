using Autodesk.Revit.UI;
using BenchmarkDotNet.Attributes;

namespace Nice3point.BenchmarkDotNet.Revit.Benchmarks;

// BenchmarkDotNet v0.15.8, Windows 11 (10.0.28000.2704/26H1/2026Update)
// AMD Ryzen 9 9950X3D 4.30GHz, 1 CPU, 32 logical and 16 physical cores
// .NET SDK 10.0.112
//   [Host]     : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v4
//   Job-YLFTCN : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v4
//
// BuildConfiguration=Release.R27  Toolchain=RevitUi
//
// | Method                                | Mean     | Error     | StdDev    | Gen0    | Gen1   | Allocated |
// |-------------------------------------- |---------:|----------:|----------:|--------:|-------:|----------:|
// | GetElementOfTypeToList                | 2.400 ms | 0.0239 ms | 0.0212 ms | 11.7188 | 7.8125 | 699.99 KB |
// | FilteredElementCollectorOfClassToList | 1.941 ms | 0.0192 ms | 0.0180 ms |  5.8594 | 3.9063 | 317.49 KB |

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class SelectionBenchmarks : RevitApiUiBenchmark
{
    private UIDocument _uiDocument = null!;

    protected sealed override void OnGlobalSetup()
    {
        var modelPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.rvt");
        var document = UiApplication.Application.NewProjectDocument(UnitSystem.Metric);

        using (var transaction = new Transaction(document, "Seed model"))
        {
            transaction.Start();

            var level = Level.Create(document, 0);
            for (var i = 0; i < 1000; i++)
            {
                Wall.Create(document, Line.CreateBound(new XYZ(i, 0, 0), new XYZ(i + 1, 0, 0)), level.Id, false);
            }

            transaction.Commit();
        }

        document.SaveAs(modelPath);
        document.Close(false);

        _uiDocument = UiApplication.OpenAndActivateDocument(modelPath);
        _uiDocument.Selection.SetElementIds(new FilteredElementCollector(_uiDocument.Document)
            .WhereElementIsNotElementType()
            .ToElementIds());
    }

    [Benchmark]
    public List<Wall> GetElementOfTypeToList()
    {
        var document = _uiDocument.Document;
        return _uiDocument.Selection.GetElementIds()
            .Select(document.GetElement)
            .OfType<Wall>()
            .ToList();
    }

    [Benchmark]
    public List<Wall> FilteredElementCollectorOfClassToList()
    {
        return new FilteredElementCollector(_uiDocument.Document, _uiDocument.Selection.GetElementIds())
            .OfClass(typeof(Wall))
            .Cast<Wall>()
            .ToList();
    }
}
