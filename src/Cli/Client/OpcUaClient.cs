using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Channels;
using Opc.Ua;
using Opc.Ua.Client;
using OpcDataValue = Opc.Ua.DataValue;

namespace Balakin.Opc.Ua.Cli.Client;

public class OpcUaClient : IDisposable
{
    private readonly string _endpoint;
    private Session? _session;

    public OpcUaClient(string endpoint)
    {
        _endpoint = endpoint;
    }

    public async Task<DataValue> ReadTagValue(string tag, CancellationToken cancellationToken)
    {
        var session = await EnsureConnected();
        var nodeId = NodeId.Parse(tag);
        var value = await session.ReadValueAsync(nodeId, cancellationToken);
        return new DataValue(value);
    }

    public async IAsyncEnumerable<DataValue> WatchTagValue(string tag, CancellationToken cancellationToken)
    {
        var session = await EnsureConnected();
        var subscription = await EnsureSubscriptionCreated(session);

        var nodeId = NodeId.Parse(tag);
        var monitoredItem = new MonitoredItem
        {
            StartNodeId = nodeId
        };

        subscription.AddItem(monitoredItem);

        var values = Channel.CreateBounded<OpcDataValue>(100);
        monitoredItem.Notification += (item, args) =>
        {
            var newValues = item.DequeueValues();
            foreach (var newValue in newValues)
            {
                values.Writer.WriteAsync(newValue, cancellationToken).GetAwaiter().GetResult();
            }
        };

        while (!cancellationToken.IsCancellationRequested)
        {
            var value = await values.Reader.ReadAsync(cancellationToken);
            yield return new DataValue(value);
        }

        subscription.RemoveItem(monitoredItem);
    }

    public void Dispose()
    {
        _session?.Dispose();
    }

    private async Task<Subscription> EnsureSubscriptionCreated(Session session)
    {
        if (session.DefaultSubscription != null)
        {
            return session.DefaultSubscription;
        }

        var subscription = new Subscription
        {
            PublishingEnabled = true
        };
        session.AddSubscription(subscription);
        session.DefaultSubscription = subscription;
        await subscription.CreateAsync();
        return subscription;
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