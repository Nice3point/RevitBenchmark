using System.Text.Json.Serialization;

namespace Nice3point.BenchmarkDotNet.Revit.Ui.Messages;

/// <summary>
///     Provides the source-generated serialization context of the UI benchmark messages.
/// </summary>
[JsonSerializable(typeof(RevitUiBenchmarkEvent))]
[JsonSerializable(typeof(RevitUiBenchmarkCommand))]
internal sealed partial class RevitUiMessageJsonContext : JsonSerializerContext;
