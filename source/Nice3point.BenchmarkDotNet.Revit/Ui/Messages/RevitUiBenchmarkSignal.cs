using BenchmarkDotNet.Engines;

namespace Nice3point.BenchmarkDotNet.Revit.Ui.Messages;

/// <summary>
///     Represents a signal of the benchmark engine that waits for a <see cref="RevitUiBenchmarkAcknowledgment" />.
/// </summary>
[PublicAPI]
internal sealed record RevitUiBenchmarkSignal : RevitUiBenchmarkEvent
{
    /// <summary>
    ///     Gets the signal of the benchmark engine.
    /// </summary>
    public required HostSignal Signal { get; init; }
}
