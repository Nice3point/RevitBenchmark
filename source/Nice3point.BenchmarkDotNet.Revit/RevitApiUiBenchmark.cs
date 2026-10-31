using Autodesk.Revit.UI;
using Nice3point.BenchmarkDotNet.Revit.Ui;

namespace Nice3point.BenchmarkDotNet.Revit;

/// <summary>
///     Represents a benchmark that runs inside the Revit user interface application.
/// </summary>
/// <remarks>
///     One Revit user interface application serves the UI benchmarks of a BenchmarkDotNet run, and it closes when the run ends.
///     Revit starts with the first UI benchmark of the run, after the benchmarks are built.
///     The benchmark methods and the <c>[GlobalSetup]</c>, <c>[GlobalCleanup]</c>, <c>[IterationSetup]</c>, and <c>[IterationCleanup]</c> methods run synchronously on the Revit thread, inside a Revit API context.
///     An asynchronous benchmark method or hook fails the validation.
///     The job settings that apply to a benchmark process, such as the runtime, the platform, the JIT, the GC settings, and the environment variables, fail the validation.
///     The console output of a benchmark goes to the log of BenchmarkDotNet.
///     The benchmarks of the run share the Revit instance, and a document a global setup opens stays open until a global cleanup closes it.
/// </remarks>
[PublicAPI]
[RevitUiConfig]
public abstract class RevitApiUiBenchmark
{
    /// <summary>
    ///     Gets the Revit user interface application.
    /// </summary>
    /// <exception cref="System.InvalidOperationException">The property is read outside the Revit user interface application.</exception>
    /// <remarks>
    ///     The application exposes the active document, the open views, and the database-level <see cref="Autodesk.Revit.ApplicationServices.Application" />.
    /// </remarks>
    protected static UIApplication UiApplication => RevitUiBenchmarkRunner.Context?.UiApplication ?? throw new InvalidOperationException("The Revit user interface application is available only inside a Revit user interface benchmark.");
}
