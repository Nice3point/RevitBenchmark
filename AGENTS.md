# Nice3point.BenchmarkDotNet.Revit Agent Instructions

Nice3point.BenchmarkDotNet.Revit is a public NuGet library that wraps BenchmarkDotNet so performance benchmarks run safely inside Revit's single-threaded API context. It ships base classes that connect a benchmark to a live Revit application and a BenchmarkDotNet job extension that aligns the benchmark build with the consumer's Revit configuration. It injects into Revit through Nice3point.Revit.Injector and PolyHook.

## Non-Negotiables

* **Run inside Revit's API context.** Benchmarks call the Revit API, so every benchmark executes on a thread connected to a live Revit application. The base classes own that connection. Never call the Revit API outside it.
* **Single-threaded API access.** The Revit API is single-threaded. The benchmark thread is configured for it. Never marshal Revit work onto another thread.
* **Author through the base classes.** A consumer derives from a base class and overrides one hook. Setup, teardown, and the Revit connection live in the base class, never in the consumer's benchmark.
* **The public surface is a contract.** Mark public types `[PublicAPI]` and read-only methods `[Pure]`. Never break an existing public API, deprecate it with `[Obsolete]` instead and keep the old member working.
* **Every type compiles under every supported Revit version.** The package multi-targets Revit 2021 through 2027. Gate version-specific Revit APIs with `#if REVIT2024_OR_GREATER`-style directives. See [Revit Best Practices](./docs/revit-best-practices.md).
* **Verify unfamiliar APIs.** When unsure of a Revit, BenchmarkDotNet, or .NET API's behavior or signature, confirm it before use. Search the web for the official docs. To read a referenced library's source, query GitHub with `gh` (`gh api`, `gh search code`). If `gh` is unavailable, search the web or ask. Never inspect compiled DLLs or XML extracted from NuGet packages.
* **Tests ship with every change.** The benchmark host project dogfoods the framework. A change that affects the run path adds or updates a benchmark there. See [Architecture](./docs/architecture.md).
* **Keep docs in sync.** A public-surface change updates `README.md`, `CHANGELOG.md`, and the XML docs in the same commit. See [Documentation](./docs/documentation.md).

## Build

The build is a ModularPipelines project. Run `dotnet run -c Release` from the `build` directory to compile.

## Specialized Docs

Read the matching file before related work.

* [Project Structure](./docs/project-structure.md). Solution layout, the shipped library, the benchmark host, and where each change belongs.
* [Architecture](./docs/architecture.md). Design goals, the base-class model, the Revit connection, the job extension, and the benchmark host.
* [Code Style](./docs/code-style.md). C# conventions, naming, attributes, language features, and the project's patterns.
* [Revit Best Practices](./docs/revit-best-practices.md). Revit API context, the single-threaded model, the version matrix, and performance.
* [Documentation](./docs/documentation.md). XML docs, README, and CHANGELOG rules.
* [Package Management](./docs/package-management.md). Centralized versions, multi-targeting, and dependencies.
