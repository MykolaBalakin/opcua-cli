namespace Balakin.CommandLine;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = true, AllowMultiple = true)]
public class CommandOptionAttribute : Attribute
{
    public Type ArgumentType { get; set; }

    public bool IsRequired { get; set; }

    public CommandOptionAttribute(Type argumentType)
    {
        ArgumentType = argumentType;
        IsRequired = true;
    }
}