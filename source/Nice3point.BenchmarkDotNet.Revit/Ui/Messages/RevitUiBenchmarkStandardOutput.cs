namespace Nice3point.BenchmarkDotNet.Revit.Ui.Messages;

/// <summary>
///     Represents a line the benchmark writes to the console.
/// </summary>
[PublicAPI]
internal sealed record RevitUiBenchmarkStandardOutput : RevitUiBenchmarkEvent
{
    /// <summary>
    ///     Gets the written line.
    /// </summary>
    public required string Line { get; init; }
}
