using Aep.Cmo.Event.Orchestrator.Infrastructure.Interfaces;
using Aep.Cmo.Shared.Domain.Models;
using Aep.Cmo.Shared.ServiceBus;
using Aep.Cmo.Shared.ServiceBus.Interfaces;
using Microsoft.Extensions.Logging;

namespace Aep.Cmo.Event.Orchestrator.Infrastructure.ServiceBus;

public class KeyVaultTopicPublisherClient<TPayload>
    : TopicPublisherClient<SyncMessage<TPayload>, TPayload>,
      IKeyVaultTopicPublisherClient<TPayload>
{
    public KeyVaultTopicPublisherClient(
        IKeyVaultServiceBusTopicOptions topicOptions,
        IServiceBusOptions serviceBusOptions,
        IServiceBusCredentialFactory credentialFactory,
        ILogger<KeyVaultTopicPublisherClient<TPayload>> logger)
        : base(topicOptions, serviceBusOptions, credentialFactory, logger)
    {
    }
}