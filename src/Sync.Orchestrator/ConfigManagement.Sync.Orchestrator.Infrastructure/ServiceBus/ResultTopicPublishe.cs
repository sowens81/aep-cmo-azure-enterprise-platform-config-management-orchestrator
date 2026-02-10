using ConfigManagement.Shared.ServiceBus;
using ConfigManagement.Shared.ServiceBus.Interfaces;
using ConfigManagement.Shared.ServiceBus.Models;
using ConfigManagement.Sync.Orchestrator.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;

namespace ConfigManagement.Sync.Orchestrator.Infrastructure.ServiceBus;

public sealed class ResultTopicPublisher<TPayload>
    : TopicPublisher<ResultMessage<TPayload>, TPayload>
{
    public ResultTopicPublisher(
        IResultServiceBusTopicOptions topic,
        IServiceBusOptions options,
        IServiceBusCredentialFactory credentialFactory,
        ILogger<TopicPublisher<ResultMessage<TPayload>, TPayload>> logger)
        : base(
            topic: topic,
            options,
            credentialFactory,
            logger)
    {
    }
}

