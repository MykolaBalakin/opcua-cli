using System.Collections;
using System.Text.Json;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;
using Balakin.CommandLine.Arguments;

namespace Balakin.CommandLine.Output;

public class XmlOutputProcessor : IOutputProcessor
{
    private readonly XPathArgument _xpath;

    public XmlOutputProcessor(XPathArgument xpath)
    {
        _xpath = xpath;
    }

    public void Process(CommandResult result, Action<string?> writeOutput)
    {
        var xmlDocument = ParseResult(result);
        var xmlObjects = SelectObjects(xmlDocument);

        foreach (var xmlObject in xmlObjects)
        {
            writeOutput(xmlObject.ToString());
        }
    }

    private XDocument ParseResult(CommandResult result)
    {
        if (result is XmlCommandResult xmlResult)
        {
            return XDocument.Parse(xmlResult.XmlContent);
        }

        if (result is ObjectCommandResult objectResult)
        {
            var jsonDocument = JsonSerializer.SerializeToDocument(
                objectResult.ObjectContent,
                objectResult.ObjectContent.GetType(),
                JsonOutputProcessor.DefaultJsonSerializerOptions);

            return new XDocument(ConvertJsonToXml(jsonDocument.RootElement, objectResult.ObjectContent.GetType().Name));
        }

        throw new ArgumentOutOfRangeException(nameof(result));

        XElement ConvertJsonToXml(JsonElement json, string name)
        {
            switch (json.ValueKind)
            {
                case JsonValueKind.Undefined:
                    return new XElement(name, "undefined");
                case JsonValueKind.Object:
                    return ConvertJsonObjectToXml(json, name);
                case JsonValueKind.Array:
                    return ConvertJsonArrayToXml(json, name);
                case JsonValueKind.String:
                    return new XElement(name, json.GetString());
                case JsonValueKind.Number:
                    return ConvertJsonNumberToXml(json, name);
                case JsonValueKind.True:
                    return new XElement(name, true);
                case JsonValueKind.False:
                    return new XElement(name, false);
                case JsonValueKind.Null:
                    return new XElement(name, "null");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        XElement ConvertJsonNumberToXml(JsonElement json, string name)
        {
            if (json.TryGetInt64(out var longValue))
            {
                return new XElement(name, longValue);
            }

            if (json.TryGetDecimal(out var decimalValue))
            {
                return new XElement(name, decimalValue);
            }

            return new XElement(name, json.GetDouble());
        }

        XElement ConvertJsonArrayToXml(JsonElement json, string name)
        {
            var items = json
                .EnumerateArray()
                .Select(item => ConvertJsonToXml(item, "Item"))
                .Cast<object>()
                .ToArray();

            return new XElement(name, items);
        }

        XElement ConvertJsonObjectToXml(JsonElement json, string name)
        {
            var items = json
                .EnumerateObject()
                .Select(item => ConvertJsonToXml(item.Value, item.Name))
                .Cast<object>()
                .ToArray();

            return new XElement(name, items);
        }
    }

    private IEnumerable<XObject> SelectObjects(XDocument document)
    {
        if (!_xpath.HasValue)
        {
            yield return document;
        }
        else
        {
            var xpathResult = document.XPathEvaluate(_xpath.Value);
            if (xpathResult is IEnumerable xpathResultEnumerable)
            {
                foreach (var item in xpathResultEnumerable)
                {
                    yield return (XObject)item;
                }
            }
            else if (xpathResult is XObject xobject)
            {
                yield return xobject;
            }
            else
            {
                throw new InvalidOperationException("XPath query returned invalid result.");
            }
        }
    }
}