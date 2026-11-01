using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Toolchains;
using BenchmarkDotNet.Toolchains.Parameters;
using BenchmarkDotNet.Toolchains.Results;
using Nice3point.BenchmarkDotNet.Revit.Sessions;
using Nice3point.BenchmarkDotNet.Revit.Ui.Messages;

namespace Nice3point.BenchmarkDotNet.Revit.Toolchains;

/// <summary>
///     Executes a built benchmark inside the Revit user interface session of the run.
/// </summary>
/// <remarks>
///     The executor reads the events of the benchmark the way BenchmarkDotNet reads the output of a benchmark process.
///     Every output line goes to the logger.
///     A host output line without the <c>//</c> prefix is a result.
///     A signal goes to the diagnoser before the executor acknowledges it.
///     The diagnoser receives the Revit process in its parameters.
/// </remarks>
internal sealed class RevitUiExecutor : IExecutor
{
    private const string PrefixedLineStart = "//";

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
            executeParameters.Logger.WriteLineError($"// Revit failed to start: {exception}");
            return CreateFailedResult(executeParameters.LaunchIndex, foundExecutable: true);
        }

        try
        {
            return Execute(session, assemblyPath, executeParameters);
        }
        catch (InvalidOperationException exception)
        {
            // The connection throws when the Revit process ends or the runner fails.
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

        var execution = new RevitUiBenchmarkExecution
        {
            AssemblyPath = assemblyPath,
            BenchmarkId = executeParameters.BenchmarkId.Value,
            BenchmarkName = FullNameProvider.GetBenchmarkName(benchmarkCase)
        };

        logger.WriteLineInfo($"// Execute in Revit process {session.Process.Id}: {assemblyPath}, benchmark {execution.BenchmarkId}");
        session.Connection.Send(execution.Serialize());

        var results = new List<string>();
        var prefixedLines = new List<string>();
        var standardOutput = new List<string>();
        while (session.Connection.ReceiveAsync().GetAwaiter().GetResult() is { } message)
        {
            switch (RevitUiBenchmarkEvent.Parse(message))
            {
                case RevitUiBenchmarkHostOutput hostOutput:
                    logger.WriteLine(LogKind.Default, hostOutput.Line);
                    if (hostOutput.Line.StartsWith(PrefixedLineStart, StringComparison.Ordinal))
                    {
                        prefixedLines.Add(hostOutput.Line);
                    }
                    else
                    {
                        results.Add(hostOutput.Line);
                    }

                    break;
                case RevitUiBenchmarkStandardOutput output:
                    logger.WriteLine(LogKind.Default, output.Line);
                    standardOutput.Add(output.Line);
                    break;
                case RevitUiBenchmarkSignal signal:
                    executeParameters.Diagnoser?.Handle(signal.Signal, diagnoserParameters);
                    session.Connection.Send(new RevitUiBenchmarkAcknowledgment().Serialize());
                    break;
                case RevitUiBenchmarkCompletion completion:
                    return new ExecuteResult(true, completion.ExitCode, session.Process.Id, results, prefixedLines, standardOutput, executeParameters.LaunchIndex);
            }
        }

        throw new InvalidOperationException("Revit closed before the benchmark finished.");
    }

    // On .NET, the managed assembly of the generated program lies next to its application host.
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
