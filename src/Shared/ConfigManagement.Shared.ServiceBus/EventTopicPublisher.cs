using System.Text.Json;
using Azure.Messaging.ServiceBus;
using ConfigManagement.Shared.ServiceBus.Authentication;
using ConfigManagement.Shared.ServiceBus.Models;
using Microsoft.Extensions.Logging;

namespace ConfigManagement.Shared.ServiceBus;

public sealed class EventTopicPublisher : IEventPublisher, IAsyncDisposable
{
    private readonly ServiceBusSender _sender;
    private readonly ILogger<EventTopicPublisher> _logger;

    public EventTopicPublisher(
        string topicName,
        ServiceBusCredentialFactory factory,
        ILogger<EventTopicPublisher> logger
    )
    {
        if (string.IsNullOrWhiteSpace(topicName))
            throw new ArgumentException("Topic name must be provided.", nameof(topicName));

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        if (factory is null)
            throw new ArgumentNullException(nameof(factory));

        var client = factory.CreateClient();
        _sender = client.CreateSender(topicName);
    }

    public async Task PublishAsync<TPayload>(
        EventMessage<TPayload> message,
        CancellationToken cancellationToken
    )
    {
        if (message is null)
            throw new ArgumentNullException(nameof(message));

        try
        {
            var body = JsonSerializer.Serialize(message);

            var serviceBusMessage = new ServiceBusMessage(body)
            {
                Subject = message.EventType,
                CorrelationId = message.CorrelationId,
                ContentType = "application/json",
            };

            serviceBusMessage.ApplicationProperties["source"] = message.Source;
            serviceBusMessage.ApplicationProperties["environmentName"] = message.EnvironmentName;
            serviceBusMessage.ApplicationProperties["traceId"] = message.TraceId;
            if (!string.IsNullOrWhiteSpace(message.SpanId))
            {
                serviceBusMessage.ApplicationProperties["spanId"] = message.SpanId;
            }

            await _sender.SendMessageAsync(serviceBusMessage, cancellationToken);

            _logger.LogInformation(
                "Published event. EventType={EventType}, CorrelationId={CorrelationId}, TraceId={TraceId}, SpanId={SpanId}, EnvironmentName={EnvironmentName}",
                message.EventType,
                message.CorrelationId,
                message.TraceId,
                message.SpanId,
                message.EnvironmentName
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to publish event. EventType={EventType}, CorrelationId={CorrelationId}, TraceId={TraceId}, SpanId={SpanId}",
                message.EventType,
                message.CorrelationId,
                message.TraceId,
                message.SpanId
            );

            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await _sender.DisposeAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error disposing ServiceBusSender");
        }
    }
}
