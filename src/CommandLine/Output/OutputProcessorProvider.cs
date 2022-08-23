namespace Balakin.CommandLine.Output;

public class OutputProcessorProvider
{
    private readonly IReadOnlyDictionary<OutputFormat, IOutputProcessor> _processors;

    public OutputProcessorProvider(
        JsonOutputProcessor jsonOutputProcessor,
        XmlOutputProcessor xmlOutputProcessor)
    {
        _processors = new Dictionary<OutputFormat, IOutputProcessor>
        {
            { OutputFormat.Json, jsonOutputProcessor },
            { OutputFormat.Xml, xmlOutputProcessor }
        };
    }

    public IOutputProcessor? Get(OutputFormat format)
    {
        if (_processors.TryGetValue(format, out var processor))
        {
            return processor;
        }

        return null;
    }
}