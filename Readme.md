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
In this case, use the `WithCurrentConfiguration()` extension for the `Job`, which will run the benchmark for your active project configuration.

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

Benchmark Revit application‑level operations:

```csharp
using BenchmarkDotNet.Attributes;

public sealed class RevitApplicationBenchmarks : RevitApiBenchmark
{
    [Benchmark]
    public double Create_XYZ_Distance()
    {
        var point = Application.Create.NewXYZ(3, 4, 5);
        return point.DistanceTo(XYZ.Zero);
    }
}
```

## Benchmarks using global hooks

BenchmarkDotNet provides **[GlobalSetup]** and **[GlobalCleanup]** hooks, but due to library limitations, it cannot be assigned twice. 
If you need these hooks in your benchmarks, for example to open the Document, use `OnSetup`/`OnCleanup` overrides instead:

```csharp
using BenchmarkDotNet.Attributes;

public sealed class RevitDocumentBenchmarks : RevitApiBenchmark
{
    private Document _documentFile = null!;

    protected sealed override void OnSetup()
    {
        _documentFile = Application.OpenDocumentFile($@"C:\Program Files\Autodesk\Revit {Application.VersionNumber}\Samples\rac_basic_sample_family.rfa");
    }
    
    protected sealed override void OnCleanup()
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
| Method                                |     Mean |   Error |   StdDev | Allocated |
|---------------------------------------|---------:|--------:|---------:|----------:|
| WhereElementIsElementTypeToElements   | 122.0 μs | 4.54 μs |  6.80 μs |   5.55 KB |
| ElementIsElementTypeFilterToElements  | 412.0 μs | 7.52 μs | 11.26 μs |   5.71 KB |
| WhereElementIsElementTypeToList       | 122.1 μs | 2.84 μs |  4.17 μs |    5.7 KB |
| WhereElementIsElementTypeCastToList   | 122.6 μs | 2.41 μs |  3.45 μs |   5.76 KB |
| WhereElementIsElementTypeOfTypeToList | 122.0 μs | 2.19 μs |  3.20 μs |   5.76 KB |