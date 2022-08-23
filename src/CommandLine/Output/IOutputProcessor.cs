using System.CommandLine.Invocation;

namespace Balakin.CommandLine.Output;

public interface IOutputProcessor
{
    void Process(CommandResult result, Action<string?> writeOutput);
}