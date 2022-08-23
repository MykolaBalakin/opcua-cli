using Balakin.CommandLine;
using Balakin.CommandLine.Output;
using Balakin.Opc.Ua.Cli.Arguments;
using Balakin.Opc.Ua.Cli.Client;

namespace Balakin.Opc.Ua.Cli.Commands.Tag;

[CommandName("read", Aliases = new[] { "get" })]
[CommandArgument(typeof(TagsArgument), IsRequired = true)]
public class ReadTagCommand : TagCommandBase
{
    private readonly TagsArgument _tags;
    private readonly OpcUaClient _client;

    public ReadTagCommand(TagsArgument tags, OpcUaClient client)
    {
        _tags = tags;
        _client = client;
    }

    public override async IAsyncEnumerable<CommandResult> Execute()
    {
        foreach (var tag in _tags.Value)
        {
            yield return await ReadTag(tag);
        }
    }

    private async Task<CommandResult> ReadTag(string tag)
    {
        var node = await _client.ReadTagValue(tag);
        return CommandResult.Object(node);
    }
}