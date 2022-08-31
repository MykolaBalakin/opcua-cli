using System.Runtime.CompilerServices;
using Balakin.CommandLine;
using Balakin.CommandLine.Output;
using Balakin.Opc.Ua.Cli.Arguments;
using Balakin.Opc.Ua.Cli.Client;

namespace Balakin.Opc.Ua.Cli.Commands.Tag;

[CommandName("read", Aliases = new[] { "get" })]
[CommandArgument(typeof(TagsArgument), IsRequired = true)]
public class ReadTagCommand : TagCommandBase
{
    public record ReadTagResult(string Tag, DataValue Value)
    {
        public override string ToString() => $"{Tag} = {Value}";
    }

    private readonly TagsArgument _tags;
    private readonly OpcUaClient _client;

    public ReadTagCommand(TagsArgument tags, OpcUaClient client)
    {
        _tags = tags;
        _client = client;
    }

    public override async IAsyncEnumerable<CommandResult> Execute([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (var tag in _tags.Value)
        {
            Exception exception=null;
            try
            {
                var value = await _client.ReadTagValue(tag, cancellationToken);
            }
            catch (Exception ex)
            {
                exception = ex;

            }

            if (exception != null)
            {
                yield return CommandResult.Object(exception);
            }

            var result = new ReadTagResult(tag, value);
            yield return CommandResult.Object(result);
        }
    }
}