using System.CommandLine.Invocation;

namespace Balakin.CommandLine.Arguments;

internal class ArgumentProvider
{
    public InvocationContext? InvocationContext { get; set; }

    public object Get(Type argumentType)
    {
        if (InvocationContext == null)
        {
            throw new InvalidOperationException(nameof(InvocationContext) + " must be set up before resolving arguments");
        }

        var argument = (ArgumentBase)Activator.CreateInstance(argumentType)!;
        argument.LoadValue(InvocationContext);
        return argument;
    }
}