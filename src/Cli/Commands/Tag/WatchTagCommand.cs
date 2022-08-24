using System.Runtime.CompilerServices;
using System.Threading.Channels;
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
        public override string ToString() => $"{Value.ServerTimestamp:s} {Tag} = {Value}";
    }

    private readonly TagsArgument _tags;
    private readonly OpcUaClient _client;

    public WatchTagCommand(TagsArgument tags, OpcUaClient client)
    {
        _tags = tags;
        _client = client;
    }

    public override async IAsyncEnumerable<CommandResult> Execute([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var channel = Channel.CreateBounded<WatchTagResult>(100);
        var tasks = new List<Task>();

        foreach (var tag in _tags.Value)
        {
            var task = Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var values = _client.WatchTagValue(tag, cancellationToken);
                    await foreach (var value in values.WithCancellation(cancellationToken))
                    {
                        var commandResult = new WatchTagResult(tag, value);
                        await channel.Writer.WriteAsync(commandResult, cancellationToken);
                    }
                }
            }, cancellationToken);
            tasks.Add(task);
        }

        while (!cancellationToken.IsCancellationRequested)
        {
            var result = await channel.Reader.ReadAsync(cancellationToken);
            yield return CommandResult.Object(result);
        }

        await Task.WhenAll(tasks);
    }
}