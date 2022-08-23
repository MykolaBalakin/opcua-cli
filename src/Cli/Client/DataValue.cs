using System.Globalization;
using System.Text.Json.Serialization;
using System.Xml.Serialization;
using Opc.Ua;
using OpcUaDataValue = Opc.Ua.DataValue;

namespace Balakin.Opc.Ua.Cli.Client;

public class DataValue
{
    [JsonIgnore]
    public OpcUaDataValue Source { get; }

    public object Value => Source.Value;
    public TypeInfo TypeInfo => Source.WrappedValue.TypeInfo;
    public DateTime ServerTimestamp => Source.ServerTimestamp;
    public DateTime SourceTimestamp => Source.SourceTimestamp;

    public DataValue(OpcUaDataValue value)
    {
        Source = value;
    }

    public override string? ToString()
    {
        return (string?)Convert.ChangeType(Value, typeof(string), CultureInfo.InvariantCulture);
    }
}