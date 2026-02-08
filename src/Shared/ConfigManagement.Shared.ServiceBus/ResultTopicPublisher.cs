using System.Text.Json;
using Azure.Messaging.ServiceBus;
using ConfigManagement.Shared.ServiceBus.Authentication;
using ConfigManagement.Shared.ServiceBus.Models;
using Microsoft.Extensions.Logging;

namespace ConfigManagement.Shared.ServiceBus;

public sealed class ResultTopicPublisher : IResultPublisher, IAsyncDisposable
{
    private readonly ServiceBusSender _sender;
    private readonly ILogger<ResultTopicPublisher> _logger;

    public ResultTopicPublisher(
        string topicName,
        ServiceBusCredentialFactory factory,
        ILogger<ResultTopicPublisher> logger
    )
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (string.IsNullOrWhiteSpace(topicName))
            throw new ArgumentException("Topic name must be provided.", nameof(topicName));

        if (factory is null)
            throw new ArgumentNullException(nameof(factory));

        var client = factory.CreateClient();
        _sender = client.CreateSender(topicName);
    }

    public async Task PublishAsync<TPayload>(
        ResultMessage<TPayload> result,
        CancellationToken cancellationToken
    )
    {
        if (result is null)
            throw new ArgumentNullException(nameof(result));

        try
        {
            var body = JsonSerializer.Serialize(result);

            var serviceBusMessage = new ServiceBusMessage(body)
            {
                CorrelationId = result.CorrelationId,
                Subject = result.EventType,
                ContentType = "application/json",
            };

            serviceBusMessage.ApplicationProperties["status"] = result.Status;
            serviceBusMessage.ApplicationProperties["source"] = result.Source;
            serviceBusMessage.ApplicationProperties["environmentName"] = result.EnvironmentName;
            serviceBusMessage.ApplicationProperties["traceId"] = result.TraceId;
            if (!string.IsNullOrWhiteSpace(result.SpanId))
            {
                serviceBusMessage.ApplicationProperties["spanId"] = result.SpanId;
            }

            if (!string.IsNullOrWhiteSpace(result.Message))
            {
                serviceBusMessage.ApplicationProperties["message"] = result.Message;
            }

            await _sender.SendMessageAsync(serviceBusMessage, cancellationToken);

            _logger.LogInformation(
                "Published result message. EventType={EventType}, CorrelationId={CorrelationId}, TraceId={TraceId}, SpanId={SpanId}, Status={Status}",
                result.EventType,
                result.CorrelationId,
                result.TraceId,
                result.SpanId,
                result.Status
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to publish result message. EventType={EventType}, CorrelationId={CorrelationId}, TraceId={TraceId}, SpanId={SpanId}",
                result.EventType,
                result.CorrelationId,
                result.TraceId,
                result.SpanId
            );

            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _sender.DisposeAsync();
    }
}
