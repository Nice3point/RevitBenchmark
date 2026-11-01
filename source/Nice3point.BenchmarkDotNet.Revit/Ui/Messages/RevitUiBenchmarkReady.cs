namespace Nice3point.BenchmarkDotNet.Revit.Ui.Messages;

/// <summary>
///     Represents the event that the Revit user interface application accepts benchmarks.
/// </summary>
[PublicAPI]
internal sealed record RevitUiBenchmarkReady : RevitUiBenchmarkEvent
{
    /// <summary>
    ///     Gets the identifier of the Revit process.
    /// </summary>
    public required int ProcessId { get; init; }
}
