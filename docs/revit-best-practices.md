# Revit Best Practices

The library connects a benchmark process to a live Revit application and runs benchmark code against the Revit API. Respect the API's rules and stay allocation-conscious, because the code under measurement runs on a hot path by definition.

## The Revit API Context

* Benchmark code calls the Revit API, so it runs against a real Revit application that the library connects to. The base classes open that connection before the session and close it after.
* The connected application is the standard Revit database-level application. Use it to create documents and reach application-wide services. Do not reach for it before the connection opens or after it closes.
* Injection requires a licensed Revit installation whose version matches the build configuration. A mismatch fails the run.
* Keep the connection and injection details inside the base classes. Do not surface them to consumers.

## The Single-Threaded Model

* The Revit API is single-threaded. The base class configures the benchmark thread for it before the connection opens.
* Run Revit work on the benchmark thread. Never marshal it onto another thread or introduce parallelism around a Revit call.
* Prepare per-session state in the global setup hook and dispose it in the global cleanup hook, so the connection stays valid for the lifetime of the work.

## Revit Versions

The active version comes from the `$(RevitVersion)` build property, and the project (SDK `Nice3point.Revit.Sdk`) declares the full `Debug.RNN`/`Release.RNN` configuration list across the supported range. The job extension reads the entry assembly's configuration so the benchmark builds against the same version.

* Use conditional compilation (`#if REVIT2024_OR_GREATER`, and similar) only where the Revit API genuinely differs between versions.
* Use `#if NET` or `#if NET8_0_OR_GREATER` for runtime differences.
* Apply directives consistently across related members so a type's surface stays coherent per version.
* Every type must compile under every declared `Debug.RNN`/`Release.RNN` configuration.
* Version-specific package versions belong in `Directory.Packages.props`. See [Package Management](./package-management.md).

## Performance

* **Keep setup out of the measured method.** Build documents and seed elements in the global setup hook, so a benchmark method measures only its target operation.
* **Avoid LINQ on hot paths** in library code. Use traditional loops where allocations or iterator overhead matter.
* **Pre-size collections** when the count is known.
* **Prefer batch Revit APIs** over per-element calls, and minimize transaction scope.
* **Release session state** in the cleanup hook so one benchmark session does not leak into the next.
