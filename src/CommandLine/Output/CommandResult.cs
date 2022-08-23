namespace Balakin.CommandLine.Output;

public abstract class CommandResult
{
    public static CommandResult Empty() => new EmptyCommandResult();
    public static CommandResult Json(string content) => new JsonCommandResult(content);
    public static CommandResult Object(object content) => new ObjectCommandResult(content);
    public static CommandResult Text(string content) => new TextCommandResult(content);
    public static CommandResult Xml(string content) => new XmlCommandResult(content);

    public abstract string? GetContent();
}

public class ObjectCommandResult : CommandResult
{
    public object ObjectContent { get; }

    public ObjectCommandResult(Object objectContent)
    {
        ObjectContent = objectContent;
    }

    public override string? GetContent() => ObjectContent.ToString();
}

public class EmptyCommandResult : CommandResult
{
    public override string GetContent() => string.Empty;
}

public class JsonCommandResult : CommandResult
{
    public string JsonContent { get; }

    public JsonCommandResult(string jsonContent)
    {
        JsonContent = jsonContent;
    }

    public override string GetContent() => JsonContent;
}

public class XmlCommandResult : CommandResult
{
    public string XmlContent { get; }

    public XmlCommandResult(string xmlContent)
    {
        XmlContent = xmlContent;
    }

    public override string GetContent() => XmlContent;
}

public class TextCommandResult : CommandResult
{
    public string TextContent { get; }

    public TextCommandResult(string textContent)
    {
        TextContent = textContent;
    }

    public override string GetContent() => TextContent;
}