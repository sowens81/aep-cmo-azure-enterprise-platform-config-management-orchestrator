using ConfigManagement.Shared.ServiceBus;
using ConfigManagement.Shared.ServiceBus.Authentication;
using ConfigManagement.Shared.ServiceBus.Interfaces;
using ConfigManagement.Shared.ServiceBus.Models;
using Microsoft.Extensions.Logging;

namespace ConfigManagement.Sync.Orchestrator.Infrastructure.ServiceBus;

public sealed class EventTopicPublisher<TPayload>
    : TopicPublisher<EventMessage<TPayload>, TPayload>
{
    public EventTopicPublisher(
        string topicName,
        IServiceBusOptions options,
        ServiceBusCredentialFactory credentialFactory,
        ILogger<TopicPublisher<EventMessage<TPayload>, TPayload>> logger)
        : base(
            topicName: topicName,
            options,
            credentialFactory,
            logger)
    {
    }
}

