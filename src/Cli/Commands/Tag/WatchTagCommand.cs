using Balakin.CommandLine;
using Balakin.CommandLine.Output;
using Balakin.Opc.Ua.Cli.Arguments;
using Balakin.Opc.Ua.Cli.Client;

namespace Balakin.Opc.Ua.Cli.Commands.Tag;

[CommandName("watch")]
[CommandArgument(typeof(TagsArgument), IsRequired = true)]
public class WatchTagCommand : TagCommandBase
{
    public record WatchTagResult(string Tag, DataValue Value)
    {
        public override string ToString() => $"{Tag} = {Value}";
    }

    private readonly TagsArgument _tags;
    private readonly OpcUaClient _client;

    public WatchTagCommand(TagsArgument tags, OpcUaClient client)
    {
        _tags = tags;
        _client = client;
    }

    public override async IAsyncEnumerable<CommandResult> Execute(CancellationToken cancellationToken)
    {
        var tag = _tags.Value.Single();
        var values = _client.WatchTagValue(tag, cancellationToken);
        await foreach (var value in values)
        {
            yield return CommandResult.Object(new WatchTagResult(tag, value));
        }
    }
}