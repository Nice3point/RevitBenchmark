using BenchmarkDotNet.EventProcessors;

namespace Nice3point.BenchmarkDotNet.Revit.Sessions;

/// <summary>
///     Closes the Revit user interface session when the BenchmarkDotNet run ends.
/// </summary>
/// <remarks>
///     BenchmarkDotNet calls one instance once per run, whatever the number of UI benchmark classes.
/// </remarks>
internal sealed class RevitUiSessionEventProcessor : EventProcessor
{
    private RevitUiSessionEventProcessor()
    {
    }

    /// <summary>
    ///     Gets the event processor instance.
    /// </summary>
    public static RevitUiSessionEventProcessor Instance { get; } = new();

    /// <inheritdoc />
    public override void OnEndRunStage()
    {
        RevitUiSession.Close();
    }
}
