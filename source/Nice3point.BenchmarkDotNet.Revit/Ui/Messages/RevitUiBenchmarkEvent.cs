using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nice3point.BenchmarkDotNet.Revit.Ui.Messages;

/// <summary>
///     Represents an event the Revit user interface application sends to the BenchmarkDotNet host process.
/// </summary>
[PublicAPI]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "event")]
[JsonDerivedType(typeof(RevitUiBenchmarkReady), "ready")]
[JsonDerivedType(typeof(RevitUiBenchmarkHostOutput), "host-output")]
[JsonDerivedType(typeof(RevitUiBenchmarkStandardOutput), "standard-output")]
[JsonDerivedType(typeof(RevitUiBenchmarkSignal), "signal")]
[JsonDerivedType(typeof(RevitUiBenchmarkCompletion), "completion")]
internal abstract record RevitUiBenchmarkEvent
{
    /// <summary>
    ///     Parses an event from a message of the Revit user interface application.
    /// </summary>
    /// <param name="message">The message to parse.</param>
    /// <returns>The parsed event.</returns>
    /// <exception cref="JsonException">The message holds no valid event.</exception>
    [Pure]
    public static RevitUiBenchmarkEvent Parse(string message)
    {
        return JsonSerializer.Deserialize(message, RevitUiMessageJsonContext.Default.RevitUiBenchmarkEvent) ?? throw new JsonException("The message holds no benchmark event.");
    }

    /// <summary>
    ///     Serializes the event into a message.
    /// </summary>
    /// <returns>The serialized event.</returns>
    [Pure]
    public string Serialize()
    {
        return JsonSerializer.Serialize(this, RevitUiMessageJsonContext.Default.RevitUiBenchmarkEvent);
    }
}
