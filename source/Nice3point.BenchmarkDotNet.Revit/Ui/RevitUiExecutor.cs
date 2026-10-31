using System.Globalization;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Toolchains;
using BenchmarkDotNet.Toolchains.Parameters;
using BenchmarkDotNet.Toolchains.Results;

namespace Nice3point.BenchmarkDotNet.Revit.Ui;

/// <summary>
///     Executes a built benchmark inside the Revit user interface application of the run.
/// </summary>
/// <remarks>
///     The executor reads the messages of the benchmark the way BenchmarkDotNet reads the pipe of a benchmark process.
///     Every line goes to the logger.
///     A line without the <c>//</c> prefix is a result, and a signal goes to the diagnoser before the executor acknowledges it.
///     The diagnoser receives the Revit process in its parameters.
/// </remarks>
internal sealed class RevitUiExecutor : IExecutor
{
    private const string LinePrefix = "//";

    /// <inheritdoc />
    public ExecuteResult Execute(ExecuteParameters executeParameters)
    {
        var assemblyPath = GetAssemblyPath(executeParameters.BuildResult.ArtifactsPaths.ExecutablePath);
        if (!File.Exists(assemblyPath))
        {
            return CreateFailedResult(executeParameters.LaunchIndex, foundExecutable: false);
        }

        RevitUiSession session;
        try
        {
            session = RevitUiSession.Open(executeParameters.Logger);
        }
        catch (Exception exception)
        {
            executeParameters.Logger.WriteLineError($"// The Revit user interface application failed to start: {exception}");
            return CreateFailedResult(executeParameters.LaunchIndex, foundExecutable: true);
        }

        try
        {
            return Execute(session, assemblyPath, executeParameters);
        }
        catch (InvalidOperationException exception)
        {
            // The connection reports a Revit process that ended or failed during the benchmark.
            executeParameters.Logger.WriteLineError($"// {exception.Message}");
            RevitUiSession.Discard(session);
            return CreateFailedResult(executeParameters.LaunchIndex, foundExecutable: true);
        }
    }

    private static ExecuteResult Execute(RevitUiSession session, string assemblyPath, ExecuteParameters executeParameters)
    {
        var logger = executeParameters.Logger;
        var benchmarkCase = executeParameters.BenchmarkCase;
        var diagnoserParameters = new DiagnoserActionParameters(session.Process, benchmarkCase, executeParameters.BenchmarkId);

        if (benchmarkCase.Job.Environment.HasValue(EnvironmentMode.AffinityCharacteristic))
        {
            session.Process.ProcessorAffinity = benchmarkCase.Job.Environment.Affinity;
        }

        var command = new RevitUiRunCommand
        {
            AssemblyPath = assemblyPath,
            BenchmarkId = executeParameters.BenchmarkId.Value,
            BenchmarkName = FullNameProvider.GetBenchmarkName(benchmarkCase)
        };

        logger.WriteLineInfo($"// Execute in the Revit process {session.Process.Id}: {assemblyPath}, benchmark {command.BenchmarkId}");
        session.Connection.Send(command.ToMessage().Format());

        var results = new List<string>();
        var prefixedLines = new List<string>();
        var standardOutput = new List<string>();
        while (session.Connection.ReceiveAsync().GetAwaiter().GetResult() is { } received)
        {
            var message = RevitUiMessage.Parse(received);
            switch (message.Kind)
            {
                case RevitUiMessage.Line:
                    logger.WriteLine(LogKind.Default, message.Payload);
                    if (!message.Payload.StartsWith(LinePrefix, StringComparison.Ordinal))
                    {
                        results.Add(message.Payload);
                    }
                    else
                    {
                        prefixedLines.Add(message.Payload);
                    }

                    break;
                case RevitUiMessage.Output:
                    logger.WriteLine(LogKind.Default, message.Payload);
                    standardOutput.Add(message.Payload);
                    break;
                case RevitUiMessage.Signal:
                    var signal = (HostSignal)Enum.Parse(typeof(HostSignal), message.Payload);
                    executeParameters.Diagnoser?.Handle(signal, diagnoserParameters);
                    session.Connection.Send(new RevitUiMessage(RevitUiMessage.Acknowledgment, string.Empty).Format());
                    break;
                case RevitUiMessage.Completed:
                    var exitCode = int.Parse(message.Payload, CultureInfo.InvariantCulture);
                    return new ExecuteResult(true, exitCode, session.Process.Id, results, prefixedLines, standardOutput, executeParameters.LaunchIndex);
            }
        }

        throw new InvalidOperationException("The Revit user interface application closed before the benchmark finished.");
    }

    // On .NET the generated program is an application host, and its managed assembly lies next to it.
    [Pure]
    private static string GetAssemblyPath(string executablePath)
    {
#if NET
        var assemblyPath = Path.ChangeExtension(executablePath, ".dll");
        return File.Exists(assemblyPath) ? assemblyPath : executablePath;
#else
        return executablePath;
#endif
    }

    [Pure]
    private static ExecuteResult CreateFailedResult(int launchIndex, bool foundExecutable)
    {
        return new ExecuteResult(foundExecutable, -1, null, [], [], [], launchIndex);
    }
}
