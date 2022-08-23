using System.CommandLine;
using System.Reflection;
using Balakin.CommandLine.Arguments;
using Balakin.CommandLine.Output;
using Microsoft.Extensions.DependencyInjection;

namespace Balakin.CommandLine;

public class CommandLineBuilder
{
    private class RootCommandBuilder
    {
        // TODO: Replace that with "required init" after it is implemented in C#
        public IReadOnlyCollection<Type> AllCommands { get; init; } = null!;
        public IServiceProvider ServiceProvider { get; init; } = null!;

        private readonly Dictionary<Type, Command> _commands = new();

        public RootCommand Build()
        {
            foreach (var commandType in AllCommands)
            {
                AddNewCommand(commandType);
            }

            return (RootCommand)_commands[typeof(CommandBase)];
        }

        private Command AddNewCommand(Type commandType)
        {
            var command = CreateEmptyCommand(commandType);
            _commands.Add(commandType, command);

            AddArguments(command, commandType);
            AddOptions(command, commandType);
            AddCommandHandler(command, commandType);

            return command;
        }

        private Command GetOrAddNewCommand(Type commandType)
        {
            if (_commands.TryGetValue(commandType, out var command))
            {
                return command;
            }

            return AddNewCommand(commandType);
        }

        private Command CreateEmptyCommand(Type commandType)
        {
            if (commandType == typeof(CommandBase))
            {
                return new RootCommand();
            }

            var commandName = commandType.GetCustomAttribute<CommandNameAttribute>(inherit: false);
            if (commandName == null)
            {
                throw new ArgumentException($"Command {commandType} must have a name. If you need to define common argument, use interfaces.", nameof(commandType));
            }

            var command = new Command(commandName.Name);

            foreach (var alias in commandName.Aliases)
            {
                command.AddAlias(alias);
            }

            if (commandType.BaseType != null)
            {
                var parentCommand = GetOrAddNewCommand(commandType.BaseType);
                parentCommand.AddCommand(command);
            }

            return command;
        }

        private void AddArguments(Command command, Type commandType)
        {
            var commandArguments = EnumerateAttributes<CommandArgumentAttribute>(commandType, false);

            foreach (var argumentAttribute in commandArguments)
            {
                var argumentDefinition = (ArgumentBase)Activator.CreateInstance(argumentAttribute.ArgumentType)!;
                var argument = new Argument();
                argument.Name = argumentDefinition.Name;
                argument.ArgumentType = argumentDefinition.Type;
                if (argumentAttribute.IsRequired)
                {
                    argument.Arity = new ArgumentArity(1, 100000);
                }

                command.AddArgument(argument);
            }
        }

        private void AddOptions(Command command, Type commandType)
        {
            var commandOptions = EnumerateAttributes<CommandOptionAttribute>(commandType, true);

            foreach (var optionAttribute in commandOptions)
            {
                var optionDefinition = (ArgumentBase)Activator.CreateInstance(optionAttribute.ArgumentType)!;
                var optionName = "--" + optionDefinition.Name;
                var option = new Option(optionName, argumentType: optionDefinition.Type);
                option.IsRequired = optionAttribute.IsRequired;
                command.AddOption(option);
            }
        }

        private void AddCommandHandler(Command command, Type commandType)
        {
            if (commandType.IsAbstract)
            {
                return;
            }

            var handler = new CommandHandler(ServiceProvider, commandType);
            command.Handler = handler;
        }

        private IEnumerable<TAttribute> EnumerateAttributes<TAttribute>(Type type, bool includeInterfaces)
            where TAttribute : Attribute
        {
            IEnumerable<TAttribute> result;

            result = type.GetCustomAttributes<TAttribute>();
            if (includeInterfaces)
            {
                result = result.Concat(type.GetInterfaces().SelectMany(i => i.GetCustomAttributes<TAttribute>()));
            }

            return result;
        }
    }

    private Assembly? _commandsAssembly;
    private Action<IServiceCollection>? _configureServices;

    public CommandLineBuilder UseCommandsFrom(Assembly assembly)
    {
        _commandsAssembly = assembly;
        return this;
    }

    public CommandLineBuilder ConfigureServices(Action<IServiceCollection> configureServices)
    {
        _configureServices = configureServices;
        return this;
    }

    public RootCommand Build() => Build(_commandsAssembly, _configureServices);

    private static RootCommand Build(Assembly? commandsAssembly, Action<IServiceCollection>? configureServices)
    {
        if (commandsAssembly == null)
        {
            throw new ArgumentNullException(nameof(commandsAssembly), "Call UseCommandsFrom to set the assembly.");
        }

        var commandTypes = commandsAssembly.GetTypes()
            .Where(t => t.IsAssignableTo(typeof(CommandBase)))
            .Where(t => !t.IsAbstract)
            .ToList();

        var services = new ServiceCollection();

        services.AddScoped<OutputProcessorProvider>();
        services.AddScoped<JsonOutputProcessor>();
        services.AddScoped<XmlOutputProcessor>();
        services.AddScoped<ArgumentProvider>();

        foreach (var command in commandTypes)
        {
            services.AddScoped(command);
        }

        var argumentTypes = commandsAssembly.GetTypes()
            .Concat(Assembly.GetExecutingAssembly().GetTypes())
            .Where(t => t.IsAssignableTo(typeof(ArgumentBase)))
            .Where(t => !t.IsAbstract);

        foreach (var argumentType in argumentTypes)
        {
            services.AddScoped(argumentType, provider => provider.GetRequiredService<ArgumentProvider>().Get(argumentType));
        }

        configureServices?.Invoke(services);

        var serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true
        });

        var rootCommandBuilder = new RootCommandBuilder
        {
            ServiceProvider = serviceProvider,
            AllCommands = commandTypes
        };

        return rootCommandBuilder.Build();
    }
}