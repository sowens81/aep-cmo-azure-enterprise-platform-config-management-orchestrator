using Azure.Core;
using Azure.Messaging.ServiceBus;
using ConfigManagement.Shared.ServiceBus.Authentication;
using ConfigManagement.Shared.ServiceBus.Interfaces;
using ConfigManagement.Shared.ServiceBus.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ConfigManagement.Shared.ServiceBus;

public class TopicPublisher<TMessage, TPayload>
    : ITopicPublisher<TMessage>, IAsyncDisposable
    where TMessage : BaseMessage<TPayload>
{
    private readonly ServiceBusSender _sender;
    private readonly ILogger<TopicPublisher<TMessage, TPayload>> _logger;

    protected TopicPublisher(
    string topicName,
    IServiceBusOptions options,
    ServiceBusCredentialFactory credentialFactory,
    ILogger<TopicPublisher<TMessage, TPayload>> logger)
    {
        if (string.IsNullOrWhiteSpace(topicName))
            throw new ArgumentException("Topic name is required.", nameof(topicName));

        if (string.IsNullOrWhiteSpace(options.Endpoint))
            throw new ArgumentException("Endpoint is required.", nameof(options.Endpoint));

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        if (credentialFactory is null)
            throw new ArgumentNullException(nameof(credentialFactory));

        // Create client first
        var client = new ServiceBusClient(
            options.Endpoint,
            credentialFactory.CreateCredential());

        // Then create a sender for the topic
        _sender = client.CreateSender(topicName);
    }

    public async Task PublishAsync(
    TMessage message,
    CancellationToken cancellationToken
    )
    {
        if (message is null)
            throw new ArgumentNullException(nameof(message));

        try
        {
            var serviceBusMessage = new ServiceBusMessage(
                BinaryData.FromObjectAsJson(message))
            {
                CorrelationId = message.CorrelationId,
                MessageId = message.CorrelationId,
                ContentType = "application/json",
            };

            // BaseMessage properties (always available)
            serviceBusMessage.ApplicationProperties["serviceName"] = message.ServiceName;
            serviceBusMessage.ApplicationProperties["traceId"] = message.TraceId;
            serviceBusMessage.ApplicationProperties["timestampUtc"] =
                message.TimestampUtc.ToString("O");

            if (!string.IsNullOrWhiteSpace(message.SpanId))
            {
                serviceBusMessage.ApplicationProperties["spanId"] = message.SpanId;
            }

            // Message-type-specific enrichment
            if (message is EventMessage<TPayload> eventMessage)
            {
                serviceBusMessage.Subject = eventMessage.EventType;
                serviceBusMessage.ApplicationProperties["source"] = eventMessage.Source;
                serviceBusMessage.ApplicationProperties["environmentName"] =
                    eventMessage.EnvironmentName;
            }
            else if (message is ResultMessage<TPayload> resultMessage)
            {
                serviceBusMessage.Subject = resultMessage.EventType;
                serviceBusMessage.ApplicationProperties["source"] = resultMessage.Source;
                serviceBusMessage.ApplicationProperties["organisation"] =
                    resultMessage.Organisation;
                serviceBusMessage.ApplicationProperties["region"] =
                    resultMessage.Region;
                serviceBusMessage.ApplicationProperties["environmentTier"] =
                    resultMessage.EnvironmentTier;
                serviceBusMessage.ApplicationProperties["environmentName"] =
                    resultMessage.EnvironmentName;
                serviceBusMessage.ApplicationProperties["status"] =
                    resultMessage.Status;
            }

            await _sender.SendMessageAsync(serviceBusMessage, cancellationToken);

            _logger.LogInformation(
                "Published {MessageType}. CorrelationId={CorrelationId}, TraceId={TraceId}",
                typeof(TMessage).Name,
                message.CorrelationId,
                message.TraceId
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to publish {MessageType}. CorrelationId={CorrelationId}",
                typeof(TMessage).Name,
                message.CorrelationId
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
