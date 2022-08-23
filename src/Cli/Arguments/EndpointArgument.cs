using Balakin.CommandLine;

namespace Balakin.Opc.Ua.Cli.Arguments;

public class EndpointArgument : ArgumentBase<string>
{
    public override string Name => "endpoint";
}