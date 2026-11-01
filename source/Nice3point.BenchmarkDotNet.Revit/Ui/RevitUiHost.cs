using System.Text;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Validators;
using Nice3point.BenchmarkDotNet.Revit.Ui.Messages;
using Nice3point.Revit.Injector.Ui;

namespace Nice3point.BenchmarkDotNet.Revit.Ui;

/// <summary>
///     Represents the BenchmarkDotNet host of a benchmark that runs inside the Revit user interface application.
/// </summary>
/// <param name="context">The context of the Revit user interface application.</param>
/// <remarks>
///     The host sends the output of the benchmark engine to the BenchmarkDotNet host process line by line.
///     <see cref="SendSignal" /> blocks the calling thread until the host process acknowledges the signal.
/// </remarks>
internal sealed class RevitUiHost(RevitUiContext context) : IHost
{
    private readonly LineWriter _hostOutput = new(context, static line => new RevitUiBenchmarkHostOutput { Line = line });

    /// <summary>
    ///     Gets the writer that sends the console output of the benchmark to the host process.
    /// </summary>
    public TextWriter StandardOutput { get; } = new LineWriter(context, static line => new RevitUiBenchmarkStandardOutput { Line = line });

    /// <inheritdoc />
    public void Write(string message)
    {
        _hostOutput.Write(message);
    }

    /// <inheritdoc />
    public void WriteLine()
    {
        _hostOutput.WriteLine();
    }

    /// <inheritdoc />
    public void WriteLine(string message)
    {
        _hostOutput.WriteLine(message);
    }

    /// <inheritdoc />
    public void SendError(string message)
    {
        _hostOutput.WriteLine($"{ValidationErrorReporter.ConsoleErrorPrefix} {message}");
    }

    /// <inheritdoc />
    public void ReportResults(RunResults runResults)
    {
        runResults.Print(_hostOutput);
    }

    /// <inheritdoc />
    /// <exception cref="NotSupportedException">The host process answered the signal with another command.</exception>
    public void SendSignal(HostSignal hostSignal)
    {
        StandardOutput.Flush();
        _hostOutput.Flush();
        context.Send(new RevitUiBenchmarkSignal { Signal = hostSignal }.Serialize());

        // The receive runs outside the synchronization context of Revit.
        // On .NET Framework, a continuation inside the receive otherwise waits for the blocked Revit thread.
        var message = Task.Run(() => context.ReceiveAsync()).GetAwaiter().GetResult();
        if (message is null && hostSignal == HostSignal.AfterAll)
        {
            return;
        }

        if (message is null || RevitUiBenchmarkCommand.Parse(message) is not RevitUiBenchmarkAcknowledgment)
        {
            throw new NotSupportedException($"Unknown acknowledgment: {message}");
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        StandardOutput.Flush();
        _hostOutput.Flush();
    }

    /// <summary>
    ///     Represents a writer that sends every complete line as an event.
    /// </summary>
    private sealed class LineWriter(RevitUiContext context, Func<string, RevitUiBenchmarkEvent> createEvent) : TextWriter
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
                    context.Send(createEvent(_line.ToString()).Serialize());
                    _line.Clear();
                    return;
                default:
                    _line.Append(value);
                    return;
            }
        }

        // Sends the incomplete line before a signal or the end of the benchmark.
        public override void Flush()
        {
            if (_line.Length > 0)
            {
                Write('\n');
            }
        }
    }
}
