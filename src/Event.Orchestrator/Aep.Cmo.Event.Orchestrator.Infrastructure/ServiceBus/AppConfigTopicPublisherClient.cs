using Aep.Cmo.Event.Orchestrator.Infrastructure.Interfaces;
using Aep.Cmo.Shared.Domain.Models;
using Aep.Cmo.Shared.ServiceBus;
using Aep.Cmo.Shared.ServiceBus.Interfaces;
using Microsoft.Extensions.Logging;

namespace Aep.Cmo.Event.Orchestrator.Infrastructure.ServiceBus;

public class AppConfigTopicPublisherClient<TPayload>
    : TopicPublisherClient<SyncMessage<TPayload>, TPayload>,
      IAppConfigTopicPublisherClient<TPayload>
{
    public AppConfigTopicPublisherClient(
        IAppConfigServiceBusTopicOptions topicOptions,
        IServiceBusOptions serviceBusOptions,
        IServiceBusCredentialFactory credentialFactory,
        ILogger<AppConfigTopicPublisherClient<TPayload>> logger)
        : base(topicOptions, serviceBusOptions, credentialFactory, logger)
    {
    }
}


