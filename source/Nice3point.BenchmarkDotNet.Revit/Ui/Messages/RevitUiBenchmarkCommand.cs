using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nice3point.BenchmarkDotNet.Revit.Ui.Messages;

/// <summary>
///     Represents a command the BenchmarkDotNet host process sends to the Revit user interface application.
/// </summary>
[PublicAPI]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "command")]
[JsonDerivedType(typeof(RevitUiBenchmarkExecution), "execute")]
[JsonDerivedType(typeof(RevitUiBenchmarkAcknowledgment), "acknowledge")]
internal abstract record RevitUiBenchmarkCommand
{
    /// <summary>
    ///     Parses a command from a message of the host process.
    /// </summary>
    /// <param name="message">The message to parse.</param>
    /// <returns>The parsed command.</returns>
    /// <exception cref="JsonException">The message holds no valid command.</exception>
    [Pure]
    public static RevitUiBenchmarkCommand Parse(string message)
    {
        return JsonSerializer.Deserialize(message, RevitUiMessageJsonContext.Default.RevitUiBenchmarkCommand) ?? throw new JsonException("The message holds no benchmark command.");
    }

    /// <summary>
    ///     Serializes the command into a message.
    /// </summary>
    /// <returns>The serialized command.</returns>
    [Pure]
    public string Serialize()
    {
        return JsonSerializer.Serialize(this, RevitUiMessageJsonContext.Default.RevitUiBenchmarkCommand);
    }
}
