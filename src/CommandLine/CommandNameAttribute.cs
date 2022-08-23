namespace Balakin.CommandLine;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class CommandNameAttribute : Attribute
{
    public string Name { get; }
    public string[] Aliases { get; set; }

    public CommandNameAttribute(string name)
    {
        Name = name;
        Aliases = Array.Empty<string>();
    }
}