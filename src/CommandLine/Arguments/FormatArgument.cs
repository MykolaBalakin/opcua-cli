using Balakin.CommandLine.Output;

namespace Balakin.CommandLine.Arguments;

public class FormatArgument : ArgumentBase<OutputFormat>
{
    public override string Name => "format";
}