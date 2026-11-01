# Unreleased

This release adds benchmarking of the Revit user interface.

## Revit UI benchmarks

`RevitApiUiBenchmark` runs benchmarks inside a Revit process started with its user interface.
The `UiApplication` property exposes the `UIApplication` of that process, and every benchmark method and hook runs on the Revit thread inside a Revit API context:

```csharp
public class SelectionBenchmarks : RevitApiUiBenchmark
{
    private UIDocument _uiDocument = null!;

    protected sealed override void OnGlobalSetup()
    {
        _uiDocument = UiApplication.OpenAndActivateDocument(modelPath);
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
```

UI benchmarks use the `RevitAPIUI` types, add the `Nice3point.Revit.Api.RevitAPIUI` package reference.

## DotTrace profiler

UI benchmarks support profiling with dotTrace.
Mark the benchmark class with the `[DotTraceDiagnoser]` attribute from the `BenchmarkDotNet.Diagnostics.dotTrace` package:

```csharp
[DotTraceDiagnoser]
public class TransactionBenchmarks : RevitApiUiBenchmark
{
    [Benchmark]
    public void SetCommentsInOneTransaction()
    {
        using var transaction = new Transaction(document, "Set comments");
        transaction.Start();

        foreach (var wall in walls)
        {
            wall.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).Set(comment);
        }

        transaction.Commit();
    }

    [Benchmark]
    public void SetCommentsInTransactionPerWall()
    {
        foreach (var wall in walls)
        {
            using var transaction = new Transaction(document, "Set comment");
            transaction.Start();
            wall.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).Set(comment);
            transaction.Commit();
        }
    }
}
```

## Bug fixes

- A benchmark project that references `RevitAPIUI` runs without `FileNotFoundException` https://github.com/Nice3point/RevitBenchmark/issues/57.

# 2027.0.1

## Revit client configuration

A benchmark runs as the Revit add-in the `RevitClient` assembly attribute declares:

```csharp
using Nice3point.Revit.Injector.Attributes;

[assembly: RevitClient(Id = "B3E7F5A2-9C4D-4E81-A6F0-2D8C51E7B934", Name = "Benchmarks", VendorId = "Contoso")]
```

Each property is optional: `Id` defaults to a random GUID, `Name` to the ID in uppercase, and `VendorId` to `Autodesk`.

## Bug fixes

- `RevitApiBenchmark` ejects Revit when `OnGlobalSetup` throws, and the benchmark process exits at the end of the run https://github.com/Nice3point/RevitBenchmark/issues/53.

# 2027.0.0

This release adds support for Revit 2027, benchmarking for different languages and custom Revit installation path.

## Localization support

BenchmarkDotNet initializes Revit with the `English - United States` language. To override these defaults, use assembly-level attributes:

- Add the attributes to any .cs file in your project (e.g., Program.cs):

    ```csharp
    using Nice3point.Revit.Injector.Attributes;

    [assembly: RevitLanguage("ENU")]
    ```

- Add the attributes directly to your .csproj file:

    ```xml
    <!-- Revit Environment Configuration -->
    <ItemGroup>
        <AssemblyAttribute Include="Nice3point.Revit.Injector.Attributes.RevitLanguageAttribute">
            <_Parameter1>ENU</_Parameter1>
        </AssemblyAttribute>
    </ItemGroup>
    ```

The `RevitLanguage` attribute accepts a [language](https://help.autodesk.com/view/RVT/2026/ENU/?guid=GUID-BD09C1B4-5520-475D-BE7E-773642EEBD6C) name (e.g., "English - United States"), code (e.g., "ENU")
or [LanguageType](https://www.revitapidocs.com/2026/dfda33cf-cbff-9fde-6672-38402e87510f.htm) enum value (e.g., "English_GB" or "15").

## Custom Revit installation path

TUnit initializes Revit from `C:\Program Files\Autodesk\Revit {version}` installation path. To override these defaults, use assembly-level attributes:

- Add the attributes to any .cs file in your project (e.g., Program.cs):

    ```csharp
    using Nice3point.Revit.Injector.Attributes;

    [assembly: RevitInstallationPath("D:\Autodesk\Revit Preview")]
    ```

- Add the attributes directly to your .csproj file:

    ```xml
    <!-- Revit Environment Configuration -->
    <ItemGroup>
        <AssemblyAttribute Include="Nice3point.Revit.Injector.Attributes.RevitInstallationPathAttribute">
            <_Parameter1>D:\Autodesk\Revit $(RevitVersion)</_Parameter1>
        </AssemblyAttribute>
    </ItemGroup>
    ```

## Breaking changes

- Global hooks `OnSetup`/`OnCleanup` renamed to `OnGlobalSetup`/`OnGlobalCleanup`

# 2026.0.0

Initial release. Enjoy!
