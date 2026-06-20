# Architecture & Design Principles

Nice3point.BenchmarkDotNet.Revit exists to make BenchmarkDotNet usable inside Revit. A benchmark that calls the Revit API needs a live Revit application and a thread that the API accepts. The library supplies both behind base classes so a consumer writes an ordinary benchmark and the Revit plumbing stays hidden.

## Core Design Goals

* **Revit-aware benchmarks.** A benchmark method runs against a real Revit application without the consumer managing the connection.
* **Simplification.** Absorb the Revit connection, the threading setup, and the BenchmarkDotNet hook wiring into base classes that expose plain overrides.
* **Single-threaded correctness.** The benchmark runs on a thread configured for Revit's single-threaded API, so Revit calls behave as they do inside the product.
* **Configuration alignment.** Match the benchmark build to the consumer's selected Revit configuration, so the measured code is the code that ships.
* **Backward compatibility.** Never break an existing public API. Deprecate with `[Obsolete]` and keep the old member working.

## Base-Class Model

A consumer derives from a base class and overrides a single hook. The base class owns the Revit connection and the BenchmarkDotNet lifecycle. The model layers two base classes so the lower level stays usable on its own.

* The application-level base class establishes the process-level prerequisites. It configures the apartment state the Revit API requires, runs the one-time module initialization that injection depends on, and exposes the connected Revit application to derived benchmarks once the connection opens. It provides the methods that open and close the connection.
* The Revit-API-level base class builds on it. It binds the BenchmarkDotNet global setup and cleanup hooks to the connection methods, opens the connection before the benchmark session, and closes it after. It exposes empty virtual hooks the consumer overrides to prepare and dispose per-session state, such as a document opened for the run.

The Revit-facing connection methods do the work. The consumer-facing overrides stay empty by default and carry no plumbing.

## The Revit Connection

The library injects into Revit rather than launching the full product UI. It uses Nice3point.Revit.Injector together with PolyHook to attach the benchmark process to a Revit application and obtain the database-level application object. The base class opens the connection at session start and ejects it at session end, so each benchmark session runs against a fresh application and releases it cleanly.

* The application object the base class exposes is the standard Revit database-level application. Derived benchmarks use it to create documents and reach application-wide services.
* The connection is process-level state owned by the base class. Do not open or close it from a consumer benchmark.
* Injection requires a licensed Revit installation whose version matches the build configuration. The README documents how a consumer points the run at a specific installation path or language.

## Job Configuration

BenchmarkDotNet defaults to a Release build, but a Revit benchmark must build under the multi-version configuration that selects the Revit API. The library provides a BenchmarkDotNet job extension that reads the entry assembly's build configuration and applies it to the job, so the benchmark compiles against the same Revit version the consumer selected. The README shows the consumer-facing call.

## The Benchmark Host

The benchmark host project is the verification surface that dogfoods the library. It derives real benchmarks from the base classes, opens a document, seeds it, and measures Revit operations end to end against a live application. A change that affects the run path adds or updates a benchmark there, because the host is how the library proves it still drives Revit correctly. There is no separate unit-test project, since the library's behavior only exists in connection with a running Revit process.

## Design Rules

* Wrap BenchmarkDotNet and the Revit connection rather than reimplement either. The base classes add lifecycle management and the Revit thread, not new benchmarking behavior.
* Keep the public surface in `[PublicAPI]`-marked types and mark read-only methods `[Pure]`.
* Keep injection and connection details inside the base classes. Do not surface them to consumers.
* Isolate version-specific Revit API differences behind compilation directives, not duplicated types. See [Revit Best Practices](./revit-best-practices.md).
