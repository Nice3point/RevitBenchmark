using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using Nice3point.BenchmarkDotNet.Revit.Sessions;
using Nice3point.BenchmarkDotNet.Revit.Toolchains;

namespace Nice3point.BenchmarkDotNet.Revit.Configs;

/// <summary>
///     Runs every job of the benchmark class inside the Revit user interface application.
/// </summary>
/// <remarks>
///     BenchmarkDotNet reads the attribute from the base class of a benchmark class.
///     The attribute applies <see cref="RevitUiToolchain" /> to every job of the class.
///     <see cref="RevitUiSessionEventProcessor" /> closes Revit when the run ends.
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
        .AddEventProcessor(RevitUiSessionEventProcessor.Instance);
}
