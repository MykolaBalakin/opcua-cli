using Balakin.CommandLine;
using Balakin.Opc.Ua.Cli.Arguments;

namespace Balakin.Opc.Ua.Cli.Commands;

[CommandOption(typeof(EndpointArgument), IsRequired = true)]
public interface IOpcUaCommand
{
}