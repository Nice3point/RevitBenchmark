using Autodesk.Revit.UI;
using BenchmarkDotNet.Attributes;

namespace Nice3point.BenchmarkDotNet.Revit.Benchmarks;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class UiDocumentBenchmarks : RevitApiUiBenchmark
{
    private UIDocument _uiDocument = null!;

    [GlobalSetup]
    public void Setup()
    {
        // The benchmarks of the run share one Revit instance, and the first setup opens the document the others reuse.
        _uiDocument = UiApplication.ActiveUIDocument ?? OpenSeededDocument();
    }

    [Benchmark]
    public UIDocument ActiveUiDocument()
    {
        return UiApplication.ActiveUIDocument;
    }

    [Benchmark]
    public ICollection<ElementId> SelectionGetElementIds()
    {
        return _uiDocument.Selection.GetElementIds();
    }

    [Benchmark]
    public IList<UIView> GetOpenUiViews()
    {
        return _uiDocument.GetOpenUIViews();
    }

    private static UIDocument OpenSeededDocument()
    {
        var documentPath = Path.Combine(Path.GetTempPath(), $"{nameof(UiDocumentBenchmarks)}.{Guid.NewGuid():N}.rvt");
        var document = UiApplication.Application.NewProjectDocument(UnitSystem.Metric);
        var wallIds = new List<ElementId>();

        using (var transaction = new Transaction(document, "Seed model"))
        {
            transaction.Start();

            var level = Level.Create(document, 0);
            for (var i = 0; i < 100; i++)
            {
                wallIds.Add(Wall.Create(document, Line.CreateBound(new XYZ(i, 0, 0), new XYZ(i + 1, 0, 0)), level.Id, false).Id);
            }

            transaction.Commit();
        }

        document.SaveAs(documentPath);
        document.Close(false);

        var uiDocument = UiApplication.OpenAndActivateDocument(documentPath);
        uiDocument.Selection.SetElementIds(wallIds);
        return uiDocument;
    }
}
