using System.Text.Json;
using System.Text.Json.Serialization;

namespace Balakin.CommandLine.Output;

public class JsonOutputProcessor : IOutputProcessor
{
    public static JsonSerializerOptions DefaultJsonSerializerOptions { get; } = new JsonSerializerOptions
    {
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    public void Process(CommandResult result, Action<string?> writeOutput)
    {
        var document = ParseResult(result);
        writeOutput(document.RootElement.ToString());
    }

    private JsonDocument ParseResult(CommandResult result)
    {
        if (result is JsonCommandResult jsonResult)
        {
            return JsonDocument.Parse(jsonResult.JsonContent);
        }

        if (result is ObjectCommandResult objectResult)
        {
            return JsonSerializer.SerializeToDocument(
                objectResult.ObjectContent,
                objectResult.ObjectContent.GetType(),
                DefaultJsonSerializerOptions);
        }

        throw new ArgumentOutOfRangeException(nameof(result));
    }
}