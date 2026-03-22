using BenchmarkDotNet.Attributes;

namespace Nice3point.BenchmarkDotNet.Revit.Tests;

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