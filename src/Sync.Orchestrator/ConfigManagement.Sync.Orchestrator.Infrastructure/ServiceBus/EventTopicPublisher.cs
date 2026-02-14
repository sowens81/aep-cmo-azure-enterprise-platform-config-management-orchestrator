using ConfigManagement.Shared.ServiceBus;
using ConfigManagement.Shared.ServiceBus.Interfaces;
using ConfigManagement.Shared.ServiceBus.Models;
using ConfigManagement.Sync.Orchestrator.Domain.Models;
using ConfigManagement.Sync.Orchestrator.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;

namespace ConfigManagement.Sync.Orchestrator.Infrastructure.ServiceBus;

public sealed class EventTopicPublisher<TPayload>
    : TopicPublisher<EventMessage<TPayload>, TPayload>
{
    public EventTopicPublisher(
        IEventServiceBusTopicOptions topic,
        IServiceBusOptions options,
        IServiceBusCredentialFactory credentialFactory,
        ILogger<TopicPublisher<EventMessage<TPayload>, TPayload>> logger)
        : base(
            topic: topic,
            options,
            credentialFactory,
            logger)
    {
    }
}

