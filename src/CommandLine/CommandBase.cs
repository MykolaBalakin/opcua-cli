using Balakin.CommandLine.Arguments;
using Balakin.CommandLine.Output;

namespace Balakin.CommandLine;

[CommandOption(typeof(FormatArgument), IsRequired = false)]
[CommandOption(typeof(XPathArgument), IsRequired = false)]
public abstract class CommandBase
{
    public abstract IAsyncEnumerable<CommandResult> Execute();
}