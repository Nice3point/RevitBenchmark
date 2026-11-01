using Autodesk.Revit.UI;
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
}
