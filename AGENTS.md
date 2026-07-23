# Nice3point.BenchmarkDotNet.Revit

Nice3point.BenchmarkDotNet.Revit is a NuGet package that runs BenchmarkDotNet tests inside a Revit process.
It owns the single thread that initializes the Revit API, marshals every benchmark and hook onto it.
It adds only the Revit execution model on top of BenchmarkDotNet; attributes, configurations stay with BenchmarkDotNet.

## Non-negotiables

* Every benchmark runs inside Revit's API context in the base class.
* The Revit API is single-threaded. The base class sets the STA apartment state before the connection opens. Never marshal Revit work onto another thread or add parallelism around a Revit call.
* Never break the public surface. Deprecate a renamed member with `[Obsolete]`, name the replacement, and keep the member functional.
* Every type compiles under every supported configuration.
* Confirm an unfamiliar Revit, BenchmarkDotNet, or .NET API before use through official docs or `gh` (`gh api`, `gh search code`).
* A public-surface change updates `README.md`, `CHANGELOG.md`, and the XML docs in the same commit.

## Execution model

* Two layered base classes. `RevitApplicationBenchmark` establishes the process prerequisites and owns the connection; `RevitApiBenchmark` binds the BenchmarkDotNet `[GlobalSetup]`/`[GlobalCleanup]` hooks to it. The connection methods stay on the application-level base and the framework-facing hooks on the API-level base. The consumer overrides only the empty `OnGlobalSetup`/`OnGlobalCleanup`; connection code never moves into a consumer override, which BenchmarkDotNet rejects as a duplicate hook.
* BenchmarkDotNet runs each benchmark in a separate executable. A `[ModuleInitializer]` sets the STA apartment state and runs the one-time injection setup.
* The `Application` is valid only between `InitializeRevitConnection` and `TerminateRevitConnection`. Never touch a Revit type outside that window.
* BenchmarkDotNet defaults to a `Release` build, which fails Revit's configurations. `WithCurrentConfiguration()` reads the entry assembly's `AssemblyConfigurationAttribute`; the benchmark then compiles against the same `Release.RNN`.

## Repository map

* `Nice3point.BenchmarkDotNet.Revit/` — the benchmark framework, packed as a NuGet package. It exposes `RevitApiBenchmark` for users.
* `Nice3point.BenchmarkDotNet.Revit.Tests/` — the runnable benchmark host that tests the library. `Program.cs` runs `BenchmarkRunner`; there is no separate unit-test project.
* `build/` — the ModularPipelines build for publishing.
* Root — `Directory.Build.props`, `Directory.Packages.props`, `global.json`, `README.md`, `CHANGELOG.md`, CI workflows.

## Build and verify

* Build: `dotnet build -c Release.R##`, where the `R##` suffix is the Revit year (`R27` targets Revit 2027).
* Test: set a Job.Dry and run `dotnet run --project Nice3point.BenchmarkDotNet.Revit.Tests -c Release.R##`; required a matching licensed Revit installation.
