using Azure.Messaging.ServiceBus;
using Aep.Cmo.Shared.ServiceBus.Interfaces;
using Aep.Cmo.Shared.ServiceBus.Models;
using Aep.Cmo.Shared.ServiceBus.OpenTelemetry;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using System.Diagnostics;

namespace Aep.Cmo.Shared.ServiceBus;

public class TopicPublisherClient<TMessage, TPayload> :
    ITopicPublisherClient<TMessage, TPayload>,
    IAsyncDisposable
    where TMessage : BaseMessage<TPayload>
{
    private static readonly TextMapPropagator Propagator =
        Propagators.DefaultTextMapPropagator;
    private readonly ServiceBusSender _sender;
    private readonly ILogger<TopicPublisherClient<TMessage, TPayload>> _logger;

    public TopicPublisherClient(
        IServiceBusTopicOptions topic,
        IServiceBusOptions options,
        IServiceBusCredentialFactory credentialFactory,
        ILogger<TopicPublisherClient<TMessage, TPayload>> logger)
    {
        if (string.IsNullOrWhiteSpace(topic.TopicName))
            throw new ArgumentException("Topic name is required.", nameof(topic.TopicName));

        if (string.IsNullOrWhiteSpace(options.Endpoint))
            throw new ArgumentException("Endpoint is required.", nameof(options.Endpoint));

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (credentialFactory is null)
            throw new ArgumentNullException(nameof(credentialFactory));

        var client = new ServiceBusClient(
            options.Endpoint,
            credentialFactory.CreateCredential());

        _sender = client.CreateSender(topic.TopicName);
    }

    public async Task PublishAsync(
        TMessage message,
        CancellationToken cancellationToken)
    {
        if (message is null)
            throw new ArgumentNullException(nameof(message));

        // Create a Producer span for this publish operation.
        using var activity = Telemetry.Source.StartActivity(
            "ServiceBus Publish",
            ActivityKind.Producer);

        try
        {
            var serviceBusMessage = new ServiceBusMessage(
                BinaryData.FromObjectAsJson(message))
            {
                CorrelationId = message.CorrelationId,
                MessageId = Guid.NewGuid().ToString(),
                ContentType = "application/json",
            };

            serviceBusMessage.ApplicationProperties["timestampUtc"] =
                message.TimestampUtc.ToString("O");

            if (activity != null)
            {
                activity.SetTag("messaging.system", "azure_service_bus");
                activity.SetTag("messaging.destination", _sender.EntityPath);
                activity.SetTag("messaging.destination_kind", "topic");
                activity.SetTag("messaging.operation", "publish");
                activity.SetTag("messaging.message_id", serviceBusMessage.MessageId);

                Propagator.Inject(
                    new PropagationContext(activity.Context, Baggage.Current),
                    serviceBusMessage,
                    static (msg, key, value) =>
                    {
                        msg.ApplicationProperties[key] = value;
                    });
            }

            await _sender.SendMessageAsync(serviceBusMessage, cancellationToken);

            _logger.LogInformation(
                "ServiceBus message published. MessageType={MessageType} MessageId={MessageId} CorrolationId={CorrolationId}",
                typeof(TMessage).Name,
                serviceBusMessage.MessageId,
                message.CorrelationId
            );
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

            _logger.LogError(
                ex,
                "Failed to publish {MessageType}. CorrolationId={CorrolationId}",
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