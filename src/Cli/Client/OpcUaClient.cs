using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Opc.Ua;
using Opc.Ua.Client;

namespace Balakin.Opc.Ua.Cli.Client;

public class OpcUaClient : IDisposable
{
    private readonly string _endpoint;
    private Session? _session;

    public OpcUaClient(string endpoint)
    {
        _endpoint = endpoint;
    }

    public async Task<object> ReadTagValue(string tag)
    {
        var session = await EnsureConnected();
        var nodeId = new NodeId(tag);
        var value = await session.ReadValueAsync(nodeId);
        return new DataValue(value);
    }

    public void Dispose()
    {
        _session?.Dispose();
    }

    private async Task<Session> EnsureConnected()
    {
        if (_session == null)
        {
            _session = await CreateNewSession();
        }

        return _session;
    }

    private async Task<Session> CreateNewSession()
    {
        var applicationConfiguration = await CreateApplicationConfiguration();
        var endpointConfiguration = EndpointConfiguration.Create(applicationConfiguration);
        var endpointDescription = CoreClientUtils.SelectEndpoint(_endpoint, false);
        var endpoint = new ConfiguredEndpoint(null, endpointDescription, endpointConfiguration);
        var userIdentity = new UserIdentity();

        return await Session.Create(
            applicationConfiguration,
            endpoint,
            false,
            false,
            applicationConfiguration.ApplicationName,
            (uint)TimeSpan.FromMinutes(10).TotalMilliseconds,
            userIdentity,
            null);
    }

    private async Task<ApplicationConfiguration> CreateApplicationConfiguration()
    {
        var applicationConfiguration = new ApplicationConfiguration
        {
            ApplicationName = "opcua-cli"
        };
        applicationConfiguration.ClientConfiguration = new ClientConfiguration
        {
        };
        applicationConfiguration.SecurityConfiguration = new SecurityConfiguration
        {
            ApplicationCertificate = new CertificateIdentifier(GenerateSelfSignedCertificate("opcua-cli"))
        };
        applicationConfiguration.CertificateValidator = new CertificateValidator
        {
        };

        await applicationConfiguration.Validate(ApplicationType.Client);
        return applicationConfiguration;
    }

    static X509Certificate2 GenerateSelfSignedCertificate(string cn)
    {
        var ecdsa = ECDsa.Create();
        var csr = new CertificateRequest("CN=" + cn, ecdsa, HashAlgorithmName.SHA256);
        var certificate = csr.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(1));
        return certificate;
    }
}