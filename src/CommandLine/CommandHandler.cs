using System.CommandLine.Invocation;
using System.CommandLine.IO;
using Balakin.CommandLine.Arguments;
using Balakin.CommandLine.Output;
using Microsoft.Extensions.DependencyInjection;

namespace Balakin.CommandLine;

public class CommandHandler : ICommandHandler
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Type _commandType;

    public CommandHandler(IServiceProvider serviceProvider, Type commandType)
    {
        _serviceProvider = serviceProvider;
        _commandType = commandType;
    }

    public async Task<int> InvokeAsync(InvocationContext context)
    {
        using var scope = _serviceProvider.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        var argumentProvider = serviceProvider.GetRequiredService<ArgumentProvider>();
        argumentProvider.InvocationContext = context;

        var command = (CommandBase)serviceProvider.GetRequiredService(_commandType);

        var results = command.Execute(context.GetCancellationToken());
        await ProcessResults(context, serviceProvider, results);

        return 0;
    }

    private async Task ProcessResults(InvocationContext context, IServiceProvider serviceProvider, IAsyncEnumerable<CommandResult> results)
    {
        await foreach (var result in results)
        {
            var formatArgument = new FormatArgument();
            formatArgument.LoadValue(context);

            var format = formatArgument.HasValue ? formatArgument.Value : OutputFormat.Text;
            var processorProvider = serviceProvider.GetRequiredService<OutputProcessorProvider>();
            var processor = processorProvider.Get(format);
            if (processor != null)
            {
                processor.Process(result, WriteOutput);
            }
            else
            {
                WriteOutput(result.GetContent());
            }
        }

        void WriteOutput(string? content)
        {
            if (content != null)
            {
                context.Console.Out.WriteLine(content);
            }
        }
    }
}