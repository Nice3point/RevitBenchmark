using Autodesk.Revit.UI;
using BenchmarkDotNet.Attributes;
using Nice3point.BenchmarkDotNet.Revit.Configs;
using Nice3point.BenchmarkDotNet.Revit.Ui;

namespace Nice3point.BenchmarkDotNet.Revit;

/// <summary>
///     Represents a benchmark that runs inside the Revit user interface application.
/// </summary>
/// <remarks>
///     One Revit application serves the UI benchmarks of a run and closes when the run ends.
///     Benchmark methods, setup methods, and cleanup methods run synchronously on the Revit thread inside a Revit API context.
///     The validation rejects asynchronous methods and the job settings of a benchmark process, such as the runtime and the GC mode.
/// </remarks>
[PublicAPI]
[RevitUiConfig]
public abstract class RevitApiUiBenchmark
{
    /// <inheritdoc cref="Autodesk.Revit.UI.UIApplication" />
    /// <exception cref="System.InvalidOperationException">The property is read outside a benchmark method, a setup method, or a cleanup method.</exception>
    protected static UIApplication UiApplication => RevitUiBenchmarkRunner.Context?.UiApplication ?? throw new InvalidOperationException("The Revit user interface application is available only inside a UI benchmark or its setup and cleanup methods.");

    /// <summary>
    ///     Runs <see cref="OnGlobalSetup" /> inside the Revit user interface application.
    /// </summary>
    /// <remarks>BenchmarkDotNet calls the method once, before the warm-up.</remarks>
    [GlobalSetup]
    public void RevitUiSessionSetup()
    {
        OnGlobalSetup();
    }

    /// <summary>
    ///     Runs <see cref="OnGlobalCleanup" /> inside the Revit user interface application.
    /// </summary>
    /// <remarks>BenchmarkDotNet calls the method once, after the last benchmark iteration.</remarks>
    [GlobalCleanup]
    public void RevitUiSessionCleanup()
    {
        OnGlobalCleanup();
    }

    /// <summary>
    ///     When overridden in a derived class, initializes the benchmark before the first iteration.
    /// </summary>
    /// <remarks>
    ///     The method runs once, before the warm-up, on the Revit thread inside a Revit API context.
    ///     After an exception from the method, <see cref="OnGlobalCleanup" /> does not run.
    /// </remarks>
    protected virtual void OnGlobalSetup()
    {
    }

    /// <summary>
    ///     When overridden in a derived class, releases the resources of the benchmark after the last iteration.
    /// </summary>
    /// <remarks>The method runs once, after the last iteration, on the Revit thread inside a Revit API context.</remarks>
    protected virtual void OnGlobalCleanup()
    {
    }
}
