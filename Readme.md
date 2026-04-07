<p align="center">
    <picture>
        <source media="(prefers-color-scheme: dark)" width="610" srcset="https://github.com/user-attachments/assets/25df81df-7d0d-45a8-842f-e8e8f65745b3">
        <img alt="RevitBenchmark" width="610" src="https://github.com/user-attachments/assets/32222c6d-c56d-4c0f-8634-1e78774d7f97">
    </picture>
</p>

## Benchmarking library for Revit

[![Nuget](https://img.shields.io/nuget/vpre/Nice3point.BenchmarkDotNet.Revit?style=for-the-badge)](https://www.nuget.org/packages/Nice3point.BenchmarkDotNet.Revit)
[![Downloads](https://img.shields.io/nuget/dt/Nice3point.BenchmarkDotNet.Revit?style=for-the-badge)](https://www.nuget.org/packages/Nice3point.BenchmarkDotNet.Revit)
[![Last Commit](https://img.shields.io/github/last-commit/Nice3point/RevitBenchmark/develop?style=for-the-badge)](https://github.com/Nice3point/RevitBenchmark/commits/develop)

Write performance benchmarks for your Revit add-ins using the [BenchmarkDotNet](https://benchmarkdotnet.org/), and share reproducible measurement experiments.

## Installation

You can install this library as a [NuGet package](https://www.nuget.org/packages/Nice3point.BenchmarkDotNet.Revit).

The packages are compiled for specific versions of Revit. To support different versions of libraries in one project, use the `RevitVersion` property:

```xml
<PackageReference Include="Nice3point.BenchmarkDotNet.Revit" Version="$(RevitVersion).*"/>
```

## Writing your first benchmark

Start by creating a new class inheriting from `RevitApiBenchmark`:

```csharp
public class MyBenchmarks : RevitApiBenchmark
{

}
```

Add one or more methods marked with `[Benchmark]`:

```csharp
using BenchmarkDotNet.Attributes;

public class MyBenchmarks : RevitApiBenchmark
{
    [Benchmark]
    public void MyBenchmark()
    {
        
    }
}
```

This is your runnable benchmark. The base class ensures the benchmark executes within Revit's single-threaded API context.

## Running your benchmarks

You can run your benchmarks with a simple configuration.
BenchmarkDotNet uses the `Release` configuration by default to run, which will cause a failure when running benchmarks for Revit, where API multi-versioning is required.
In this case, use the `WithCurrentConfiguration()` extension for the `Job`, which will run the benchmark for your selected Solution configuration.

```csharp
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using Nice3point.BenchmarkDotNet.Revit;

var configuration = ManualConfig.Create()
    .AddJob(Job.Default.WithCurrentConfiguration());

BenchmarkRunner.Run<MyBenchmarks>(configuration);
```

> [!IMPORTANT]
> You must have a licensed copy of Autodesk Revit installed on your machine to run benchmarks, with a version that matches the selected Solution configuration.

## Application-level benchmarks

Benchmark Revit application-level operations:

```csharp
using BenchmarkDotNet.Attributes;

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
```

## Document benchmarks

Use `OnGlobalSetup` and `OnGlobalCleanup` overrides to open and close a document around benchmark iterations.
BenchmarkDotNet provides `[GlobalSetup]` and `[GlobalCleanup]` hooks, but due to library limitations, they cannot be assigned twice:

```csharp
using BenchmarkDotNet.Attributes;

public class RevitCollectorBenchmarks : RevitApiBenchmark
{
    private Document _document = null!;

    protected sealed override void OnGlobalSetup()
    {
        _document = Application.NewProjectDocument(UnitSystem.Metric);
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
    public List<Element> WhereElementIsNotElementTypeToList()
    {
        return new FilteredElementCollector(_document)
            .WhereElementIsNotElementType()
            .ToList();
    }
}
```

```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.22631.6199/23H2/2023Update/SunValley3)
AMD Ryzen 5 5600 3.50GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 10.0.100
  [Host]    : .NET 8.0.22 (8.0.22, 8.0.2225.52707), X64 RyuJIT x86-64-v3
  MediumRun : .NET 8.0.22 (8.0.22, 8.0.2225.52707), X64 RyuJIT x86-64-v3

Job=MediumRun  BuildConfiguration=Release.R26  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method                                   |     Mean |     Error |    StdDev | Allocated |
|------------------------------------------|---------:|----------:|----------:|----------:|
| WhereElementIsNotElementTypeToElements   | 2.203 ms | 0.0494 ms | 0.1440 ms | 295.05 KB |
| WhereElementIsNotElementTypeToList       | 2.190 ms | 0.0436 ms | 0.0929 ms | 310.09 KB |

## Benchmark configuration

BenchmarkDotNet initializes Revit with the `English - United States` language and the `C:\Program Files\Autodesk\Revit {version}` installation path. To override these defaults:

- Add the assembly-level attribute to any .cs file in your project (e.g., Program.cs):

    ```csharp
    using Nice3point.Revit.Injector.Attributes;
    
    [assembly: RevitLanguage("ENU")]
    [assembly: RevitInstallationPath("D:\Autodesk\Revit Preview")]
    ```

- Or add the attributes directly to your .csproj file:

    ```xml
    <!-- Revit Environment Configuration -->
    <ItemGroup>
  
        <AssemblyAttribute Include="Nice3point.Revit.Injector.Attributes.RevitLanguageAttribute">
            <_Parameter1>ENU</_Parameter1>
        </AssemblyAttribute>
  
        <AssemblyAttribute Include="Nice3point.Revit.Injector.Attributes.RevitInstallationPathAttribute">
            <_Parameter1>D:\Autodesk\Revit $(RevitVersion)</_Parameter1>
        </AssemblyAttribute>
  
    </ItemGroup>
    ```

The `RevitLanguage` attribute accepts a [language](https://help.autodesk.com/view/RVT/2026/ENU/?guid=GUID-BD09C1B4-5520-475D-BE7E-773642EEBD6C) name (e.g., "English - United States"), code (e.g., "ENU")
or [LanguageType](https://www.revitapidocs.com/2026/dfda33cf-cbff-9fde-6672-38402e87510f.htm) enum value (e.g., "English_GB" or "15").