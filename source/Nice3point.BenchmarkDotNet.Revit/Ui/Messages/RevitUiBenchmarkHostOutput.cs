namespace Nice3point.BenchmarkDotNet.Revit.Ui.Messages;

/// <summary>
///     Represents a line the benchmark engine writes to its host.
/// </summary>
[PublicAPI]
internal sealed record RevitUiBenchmarkHostOutput : RevitUiBenchmarkEvent
{
    /// <summary>
    ///     Gets the written line.
    /// </summary>
    public required string Line { get; init; }
}
