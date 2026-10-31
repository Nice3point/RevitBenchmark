using BenchmarkDotNet.Configs;
using BenchmarkDotNet.EventProcessors;
using BenchmarkDotNet.Jobs;

namespace Nice3point.BenchmarkDotNet.Revit.Ui;

/// <summary>
///     Runs every job of a benchmark class inside the Revit user interface application, and closes Revit when the run ends.
/// </summary>
/// <remarks>
///     BenchmarkDotNet reads the attribute from the base class of a benchmark class, and applies the toolchain to every job of the run for that class.
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
internal sealed class RevitUiConfigAttribute : Attribute, IConfigSource
{
    /// <inheritdoc />
    public IConfig Config { get; } = ManualConfig.CreateEmpty()
        .AddJob(new Job
            {
                Infrastructure =
                {
                    Toolchain = RevitUiToolchain.Instance
                }
            }
            .AsMutator())
        .AddEventProcessor(SessionCloser.Instance);

    /// <summary>
    ///     Closes the Revit user interface application when the run ends.
    /// </summary>
    /// <remarks>
    ///     BenchmarkDotNet calls one instance once, whatever the number of benchmark classes.
    /// </remarks>
    private sealed class SessionCloser : EventProcessor
    {
        public static SessionCloser Instance { get; } = new();

        public override void OnEndRunStage()
        {
            RevitUiSession.Close();
        }
    }
}
