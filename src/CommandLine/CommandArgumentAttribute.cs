namespace Balakin.CommandLine;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public class CommandArgumentAttribute : Attribute
{
    public Type ArgumentType { get; set; }

    public bool IsRequired { get; set; }

    public CommandArgumentAttribute(Type argumentType)
    {
        ArgumentType = argumentType;
        IsRequired = true;
    }
}