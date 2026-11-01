namespace Nice3point.BenchmarkDotNet.Revit.Ui.Messages;

/// <summary>
///     Represents the event that one benchmark finished.
/// </summary>
[PublicAPI]
internal sealed record RevitUiBenchmarkCompletion : RevitUiBenchmarkEvent
{
    /// <summary>
    ///     Gets the exit code of the generated benchmark program.
    /// </summary>
    /// <value><c>0</c> for a finished benchmark; otherwise, <c>-1</c>.</value>
    public required int ExitCode { get; init; }
}
