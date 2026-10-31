using System.Text;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Validators;
using Nice3point.Revit.Injector.Ui;

namespace Nice3point.BenchmarkDotNet.Revit.Ui;

/// <summary>
///     Represents the BenchmarkDotNet host of a benchmark that runs inside the Revit user interface application.
/// </summary>
/// <param name="context">The context of the Revit user interface application.</param>
/// <remarks>
///     The host sends every line to the BenchmarkDotNet host process, the way a generated benchmark program writes it to its pipe.
///     <see cref="SendSignal" /> blocks the calling thread until the host process acknowledges the signal.
/// </remarks>
internal sealed class RevitUiHost(RevitUiContext context) : IHost
{
    private readonly LineWriter _engineWriter = new(context, RevitUiMessage.Line);

    /// <summary>
    ///     Gets the writer that sends the console output of the benchmark to the host process.
    /// </summary>
    public TextWriter Output { get; } = new LineWriter(context, RevitUiMessage.Output);

    /// <inheritdoc />
    public void Write(string message)
    {
        _engineWriter.Write(message);
    }

    /// <inheritdoc />
    public void WriteLine()
    {
        _engineWriter.WriteLine();
    }

    /// <inheritdoc />
    public void WriteLine(string message)
    {
        _engineWriter.WriteLine(message);
    }

    /// <inheritdoc />
    public void SendError(string message)
    {
        _engineWriter.WriteLine($"{ValidationErrorReporter.ConsoleErrorPrefix} {message}");
    }

    /// <inheritdoc />
    public void ReportResults(RunResults runResults)
    {
        runResults.Print(_engineWriter);
    }

    /// <inheritdoc />
    /// <exception cref="NotSupportedException">The host process answered the signal with a message other than an acknowledgment.</exception>
    public void SendSignal(HostSignal hostSignal)
    {
        Output.Flush();
        _engineWriter.Flush();
        context.Send(new RevitUiMessage(RevitUiMessage.Signal, hostSignal.ToString()).Format());

        // The Revit thread blocks on the receive, and the receive starts outside the synchronization context of Revit.
        // On .NET Framework, a continuation inside the receive otherwise waits for the blocked Revit thread.
        var acknowledgment = Task.Run(() => context.ReceiveAsync()).GetAwaiter().GetResult();
        if (acknowledgment is null && hostSignal == HostSignal.AfterAll)
        {
            return;
        }

        if (acknowledgment is null || RevitUiMessage.Parse(acknowledgment).Kind != RevitUiMessage.Acknowledgment)
        {
            throw new NotSupportedException($"Unknown acknowledgment: {acknowledgment}");
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Output.Flush();
        _engineWriter.Flush();
    }

    /// <summary>
    ///     Sends every complete line written to it as a message of one kind.
    /// </summary>
    private sealed class LineWriter(RevitUiContext context, string kind) : TextWriter
    {
        private readonly StringBuilder _line = new();

        public override Encoding Encoding => Encoding.UTF8;

        public override void Write(char value)
        {
            switch (value)
            {
                case '\r':
                    return;
                case '\n':
                    context.Send(new RevitUiMessage(kind, _line.ToString()).Format());
                    _line.Clear();
                    return;
                default:
                    _line.Append(value);
                    return;
            }
        }

        // Sends the incomplete line, which the next signal or the end of the benchmark would otherwise hold back.
        public override void Flush()
        {
            if (_line.Length > 0)
            {
                Write('\n');
            }
        }
    }
}
