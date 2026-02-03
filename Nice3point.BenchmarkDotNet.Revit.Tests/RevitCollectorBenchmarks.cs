using BenchmarkDotNet.Attributes;

namespace Nice3point.BenchmarkDotNet.Revit.Tests;

[MemoryDiagnoser]
public class RevitCollectorBenchmarks : RevitApiBenchmark
{
    private Document _documentFile = null!;

    protected sealed override void OnGlobalSetup()
    {
        _documentFile = Application.OpenDocumentFile($@"C:\Program Files\Autodesk\Revit {Application.VersionNumber}\Samples\rac_basic_sample_family.rfa");
    }
    
    protected sealed override void OnGlobalCleanup()
    {
        _documentFile.Close(false);
    }

    [Benchmark]
    public IList<Element> WhereElementIsElementTypeToElements()
    {
        return new FilteredElementCollector(_documentFile)
            .WhereElementIsElementType()
            .ToElements();
    }

    [Benchmark]
    public IList<Element> ElementIsElementTypeFilterToElements()
    {
        return new FilteredElementCollector(_documentFile)
            .WherePasses(new ElementIsElementTypeFilter())
            .ToElements();
    }

    [Benchmark]
    public List<Element> WhereElementIsElementTypeToList()
    {
        return new FilteredElementCollector(_documentFile)
            .WhereElementIsElementType()
            .ToList();
    }

    [Benchmark]
    public List<ElementType> WhereElementIsElementTypeCastToList()
    {
        return new FilteredElementCollector(_documentFile)
            .WhereElementIsElementType()
            .Cast<ElementType>()
            .ToList();
    }

    [Benchmark]
    public List<ElementType> WhereElementIsElementTypeOfTypeToList()
    {
        return new FilteredElementCollector(_documentFile)
            .WhereElementIsElementType()
            .OfType<ElementType>()
            .ToList();
    }
}