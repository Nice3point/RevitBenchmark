namespace Nice3point.BenchmarkDotNet.Revit.Ui.Messages;

/// <summary>
///     Represents the command that executes one benchmark of a generated benchmark program.
/// </summary>
[PublicAPI]
internal sealed record RevitUiBenchmarkExecution : RevitUiBenchmarkCommand
{
    /// <summary>
    ///     Gets the absolute path of the generated benchmark assembly.
    /// </summary>
    public required string AssemblyPath { get; init; }

    /// <summary>
    ///     Gets the identifier of the benchmark inside the generated assembly.
    /// </summary>
    public required int BenchmarkId { get; init; }

    /// <summary>
    ///     Gets the full name of the benchmark.
    /// </summary>
    public required string BenchmarkName { get; init; }
}
