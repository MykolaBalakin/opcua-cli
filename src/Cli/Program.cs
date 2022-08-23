using System.CommandLine;
using System.Reflection;
using Balakin.CommandLine;
using Balakin.Opc.Ua.Cli.Client;
using Balakin.Opc.Ua.Cli.Commands;
using Microsoft.Extensions.DependencyInjection;

var commandsAssembly = Assembly.GetExecutingAssembly();
var commandLine = BuildCommandLine();
return await commandLine.InvokeAsync(args);

Command BuildCommandLine()
{
    var builder = new CommandLineBuilder();
    return builder
        .UseCommandsFrom(commandsAssembly)
        .ConfigureServices(ConfigureServices)
        .Build();
}

void ConfigureServices(IServiceCollection services)
{
    services.AddScoped<OpcUaClientFactory>();
    services.AddScoped(serviceProvider => serviceProvider.GetRequiredService<OpcUaClientFactory>().Create());
}