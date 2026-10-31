using System.Globalization;

namespace Nice3point.BenchmarkDotNet.Revit.Ui;

/// <summary>
///     Represents the command that runs one benchmark of a generated benchmark program inside the Revit user interface application.
/// </summary>
internal sealed record RevitUiRunCommand
{
    /// <summary>
    ///     Gets the absolute path of the generated benchmark assembly.
    /// </summary>
    public required string AssemblyPath { get; init; }

    /// <summary>
    ///     Gets the identifier of the benchmark inside the generated assembly.
    /// </summary>
    public required int BenchmarkId { get; init; }

    /// <summary>
    ///     Gets the full name of the benchmark.
    /// </summary>
    public required string BenchmarkName { get; init; }

    /// <summary>
    ///     Reads the command from the payload of a <see cref="RevitUiMessage.Run" /> message.
    /// </summary>
    /// <param name="payload">The payload of the message.</param>
    /// <returns>The command.</returns>
    [Pure]
    public static RevitUiRunCommand Parse(string payload)
    {
        var lines = payload.Split('\n');
        return new RevitUiRunCommand
        {
            AssemblyPath = lines[0],
            BenchmarkId = int.Parse(lines[1], CultureInfo.InvariantCulture),
            BenchmarkName = lines[2]
        };
    }

    /// <summary>
    ///     Creates the <see cref="RevitUiMessage.Run" /> message of the command.
    /// </summary>
    /// <returns>The message.</returns>
    [Pure]
    public RevitUiMessage ToMessage()
    {
        return new RevitUiMessage(RevitUiMessage.Run, $"{AssemblyPath}\n{BenchmarkId.ToString(CultureInfo.InvariantCulture)}\n{BenchmarkName}");
    }
}
