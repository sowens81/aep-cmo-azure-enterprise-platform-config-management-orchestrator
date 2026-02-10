using ConfigManagement.Shared.ServiceBus;
using ConfigManagement.Shared.ServiceBus.Authentication;
using ConfigManagement.Shared.ServiceBus.Interfaces;
using ConfigManagement.Shared.ServiceBus.Models;
using Microsoft.Extensions.Logging;

namespace ConfigManagement.Sync.Orchestrator.Infrastructure.ServiceBus;

public sealed class ResultTopicPublisher<TPayload>
    : TopicPublisher<ResultMessage<TPayload>, TPayload>
{
    public ResultTopicPublisher(
        string topicName,
        IServiceBusOptions options,
        ServiceBusCredentialFactory credentialFactory,
        ILogger<TopicPublisher<ResultMessage<TPayload>, TPayload>> logger)
        : base(
            topicName: topicName,
            options,
            credentialFactory,
            logger)
    {
    }
}

