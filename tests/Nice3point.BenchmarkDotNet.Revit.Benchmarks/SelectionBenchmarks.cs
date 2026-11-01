using Autodesk.Revit.UI;
using BenchmarkDotNet.Attributes;

namespace Nice3point.BenchmarkDotNet.Revit.Benchmarks;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class SelectionBenchmarks : RevitApiUiBenchmark
{
    private UIDocument _uiDocument = null!;

    [GlobalSetup]
    public void SeedModel()
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
