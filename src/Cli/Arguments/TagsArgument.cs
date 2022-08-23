using Balakin.CommandLine;

namespace Balakin.Opc.Ua.Cli.Arguments;

public class TagsArgument : ArgumentBase<string[]>
{
    public override string Name => "tags";
}