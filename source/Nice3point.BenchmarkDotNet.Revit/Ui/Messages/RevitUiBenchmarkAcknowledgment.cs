namespace Nice3point.BenchmarkDotNet.Revit.Ui.Messages;

/// <summary>
///     Represents the command that acknowledges a <see cref="RevitUiBenchmarkSignal" />.
/// </summary>
[PublicAPI]
internal sealed record RevitUiBenchmarkAcknowledgment : RevitUiBenchmarkCommand;
