using System.Diagnostics;
using System.Globalization;
using BenchmarkDotNet.Loggers;
using Nice3point.Revit.Injector;
using Nice3point.Revit.Injector.Ui;

namespace Nice3point.BenchmarkDotNet.Revit.Ui;

/// <summary>
///     Represents the Revit user interface application that runs the UI benchmarks of one BenchmarkDotNet run.
/// </summary>
/// <remarks>
///     The first UI benchmark of a run starts Revit, every later UI benchmark of the run reuses it, and <see cref="Close" /> closes it when the run ends.
///     A Revit instance that ends during a benchmark is replaced by a new one for the next benchmark.
///     A Revit instance that fails to start fails every later UI benchmark of the run with the same error.
///     The members are safe to call from any thread.
/// </remarks>
internal sealed class RevitUiSession
{
    private static readonly TimeSpan CloseTimeout = TimeSpan.FromSeconds(30);
    private static readonly Lock SessionLock = new();
    private static RevitUiSession? _current;
    private static Exception? _startupFailure;

    private RevitUiSession(RevitUiConnection connection, Process process)
    {
        Connection = connection;
        Process = process;
    }

    /// <summary>
    ///     Gets the connection to the Revit user interface application.
    /// </summary>
    public RevitUiConnection Connection { get; }

    /// <summary>
    ///     Gets the Revit process.
    /// </summary>
    public Process Process { get; }

    /// <summary>
    ///     Gets the open session, and starts Revit when no session is open.
    /// </summary>
    /// <param name="logger">The logger of the run.</param>
    /// <returns>The open session.</returns>
    /// <exception cref="System.InvalidOperationException">Revit failed to start in this run.</exception>
    public static RevitUiSession Open(ILogger logger)
    {
        lock (SessionLock)
        {
            if (_startupFailure is not null)
            {
                throw new InvalidOperationException("The Revit user interface application failed to start earlier in this run.", _startupFailure);
            }

            if (_current is not null)
            {
                return _current;
            }

            try
            {
                return _current = Start(logger);
            }
            catch (Exception exception)
            {
                _startupFailure = exception;
                throw;
            }
        }
    }

    /// <summary>
    ///     Discards the session after its Revit instance ended.
    /// </summary>
    /// <param name="session">The session to discard.</param>
    public static void Discard(RevitUiSession session)
    {
        lock (SessionLock)
        {
            if (_current == session)
            {
                _current = null;
            }
        }

        session.Dispose(TimeSpan.Zero);
    }

    /// <summary>
    ///     Closes the open session once Revit finishes, and ends Revit when it does not finish in time.
    /// </summary>
    /// <remarks>
    ///     A call without an open session has no effect.
    ///     The next UI benchmark starts a new session.
    /// </remarks>
    public static void Close()
    {
        RevitUiSession? session;
        lock (SessionLock)
        {
            session = _current;
            _current = null;
            _startupFailure = null;
        }

        session?.Dispose(CloseTimeout);
    }

    private static RevitUiSession Start(ILogger logger)
    {
        logger.WriteLineInfo("// Starting the Revit user interface application");

        var stopwatch = Stopwatch.StartNew();
        var options = new RevitUiApplicationOptions
        {
            EntryPoint = typeof(RevitUiBenchmarkRunner)
        };

        var connection = new Injector().InjectUiApplicationAsync(options).GetAwaiter().GetResult();
        try
        {
            var message = connection.ReceiveAsync().GetAwaiter().GetResult();
            if (message is null || RevitUiMessage.Parse(message) is not { Kind: RevitUiMessage.Ready } ready)
            {
                throw new InvalidOperationException($"The Revit user interface application sent an unexpected message: {message}");
            }

            var process = Process.GetProcessById(int.Parse(ready.Payload, CultureInfo.InvariantCulture));
            TrySetHighPriority(process, logger);

            logger.WriteLineInfo($"// The Revit user interface application started in {stopwatch.Elapsed.TotalSeconds:F1} s, process {process.Id}");
            return new RevitUiSession(connection, process);
        }
        catch
        {
            connection.DisposeAsync().AsTask().GetAwaiter().GetResult();
            throw;
        }
    }

    private static void TrySetHighPriority(Process process, ILogger logger)
    {
        try
        {
            process.PriorityClass = ProcessPriorityClass.High;
        }
        catch (Exception exception) when (exception is InvalidOperationException or System.ComponentModel.Win32Exception)
        {
            logger.WriteLineInfo($"// Failed to set the high priority of the Revit process: {exception.Message}");
        }
    }

    private void Dispose(TimeSpan timeout)
    {
        try
        {
            Connection.CloseAsync(timeout).GetAwaiter().GetResult();
        }
        finally
        {
            Process.Dispose();
        }
    }
}
