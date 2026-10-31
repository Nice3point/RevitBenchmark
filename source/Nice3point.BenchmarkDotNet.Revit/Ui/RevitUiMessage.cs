namespace Nice3point.BenchmarkDotNet.Revit.Ui;

/// <summary>
///     Represents a message between the BenchmarkDotNet host process and the Revit user interface application.
/// </summary>
/// <param name="Kind">The kind of the message, one of the constants of <see cref="RevitUiMessage" />.</param>
/// <param name="Payload">The content of the message.</param>
/// <remarks>
///     The host process sends <see cref="Run" /> and <see cref="Acknowledgment" />.
///     Revit sends <see cref="Ready" /> once, then <see cref="Line" />, <see cref="Output" />, <see cref="Signal" />, and <see cref="Completed" /> for every benchmark.
/// </remarks>
internal readonly record struct RevitUiMessage(string Kind, string Payload)
{
    /// <summary>
    ///     Starts a benchmark. The payload holds the assembly path, the benchmark identifier, and the benchmark name, one per line.
    /// </summary>
    public const string Run = "run";

    /// <summary>
    ///     Acknowledges a <see cref="Signal" />. The payload is empty.
    /// </summary>
    public const string Acknowledgment = "ack";

    /// <summary>
    ///     Reports that Revit accepts benchmarks. The payload holds the identifier of the Revit process.
    /// </summary>
    public const string Ready = "ready";

    /// <summary>
    ///     Carries a line the benchmark engine writes. The payload holds the line.
    /// </summary>
    public const string Line = "line";

    /// <summary>
    ///     Carries a line the benchmark writes to the console. The payload holds the line.
    /// </summary>
    public const string Output = "output";

    /// <summary>
    ///     Carries a <see cref="global::BenchmarkDotNet.Engines.HostSignal" /> that waits for an <see cref="Acknowledgment" />. The payload holds the signal name.
    /// </summary>
    public const string Signal = "signal";

    /// <summary>
    ///     Reports the end of a benchmark. The payload holds the exit code.
    /// </summary>
    public const string Completed = "completed";

    private const char Separator = '\n';

    /// <summary>
    ///     Parses a message received through the channel.
    /// </summary>
    /// <param name="message">The received message.</param>
    /// <returns>The parsed message.</returns>
    [Pure]
    public static RevitUiMessage Parse(string message)
    {
        var separatorIndex = message.IndexOf(Separator);
        return separatorIndex < 0
            ? new RevitUiMessage(message, string.Empty)
            : new RevitUiMessage(message[..separatorIndex], message[(separatorIndex + 1)..]);
    }

    /// <summary>
    ///     Formats the message for the channel.
    /// </summary>
    /// <returns>The formatted message.</returns>
    [Pure]
    public string Format()
    {
        return $"{Kind}{Separator}{Payload}";
    }
}
