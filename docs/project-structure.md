# Project Structure

Nice3point.BenchmarkDotNet.Revit wraps BenchmarkDotNet so benchmarks run inside Revit's API context. The solution separates the shipped library from the benchmark host that exercises it and from the build automation. Keep each piece of code in the project that owns its responsibility.

## Solution Groups

* **The library project**: the shipped NuGet package. It holds the benchmark base classes a consumer derives from and the BenchmarkDotNet extensions that adapt a job to Revit. It is the only package-producing project, and it multi-targets every supported Revit version.
* **The benchmark host project**: the runnable executable that dogfoods the library. It derives real benchmarks from the base classes and runs them against a live Revit application, so it doubles as the verification surface and the worked example.
* **`/build`**: the ModularPipelines build that compiles, tests, packages, and publishes the package.
* **Root**: build and package configuration, the README and CHANGELOG, the agent guidelines, and the CI workflows.

## Change Placement

* A benchmark base class goes in the library project alongside the other base classes, ordered from the lower application-level base to the higher Revit-API-level base.
* A BenchmarkDotNet configuration helper, such as a job extension, goes under the library's extensions folder.
* The code that injects into Revit and manages the application connection stays in the base classes, never in a consumer-facing helper.
* A new benchmark that proves a code path or demonstrates the library goes in the benchmark host project.
* Build, packaging, and publishing logic goes in a module under the build project.
