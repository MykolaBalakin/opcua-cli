using Balakin.Opc.Ua.Cli.Arguments;
using Balakin.Opc.Ua.Cli.Client;

namespace Balakin.Opc.Ua.Cli.Commands;

public class OpcUaClientFactory
{
    private readonly EndpointArgument _endpointArgument;

    public OpcUaClientFactory(EndpointArgument endpointArgument)
    {
        _endpointArgument = endpointArgument;
    }

    public OpcUaClient Create()
    {
        return new OpcUaClient(_endpointArgument.Value);
    }
}