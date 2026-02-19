using Aep.Cmo.Shared.ServiceBus.Interfaces;
using Aep.Cmo.Shared.ServiceBus.Messaging;
using Aep.Cmo.Shared.ServiceBus.OpenTelemetry;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using System.Diagnostics;

namespace Aep.Cmo.Shared.ServiceBus;

/// <summary>
/// Publishes strongly-typed messages to an Azure Service Bus topic
/// with OpenTelemetry distributed tracing support.
/// </summary>
/// <typeparam name="TPayload">
/// The type of the message payload.
/// </typeparam>
public sealed class TopicPublisherClient<TPayload> :
    ITopicPublisherClient<TPayload>,
    IAsyncDisposable
{
    private static readonly TextMapPropagator Propagator =
        Propagators.DefaultTextMapPropagator;

    private readonly ServiceBusSender _sender;
    private readonly ILogger<TopicPublisherClient<TPayload>> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TopicPublisherClient{TPayload}"/> class.
    /// </summary>
    public TopicPublisherClient(
        IServiceBusTopicOptions topic,
        IServiceBusOptions options,
        IServiceBusCredentialFactory credentialFactory,
        ILogger<TopicPublisherClient<TPayload>> logger)
    {
        ArgumentNullException.ThrowIfNull(topic);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(credentialFactory);
        ArgumentNullException.ThrowIfNull(logger);

        if (string.IsNullOrWhiteSpace(topic.TopicName))
            throw new ArgumentException("Topic name is required.", nameof(topic.TopicName));

        if (string.IsNullOrWhiteSpace(options.Endpoint))
            throw new ArgumentException("Endpoint is required.", nameof(options.Endpoint));

        _logger = logger;

        var client = new ServiceBusClient(
            options.Endpoint,
            credentialFactory.CreateCredential());

        _sender = client.CreateSender(topic.TopicName);
    }

    /// <inheritdoc />
    public async Task PublishAsync(
        TopicMessage<TPayload> message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        using var activity = Telemetry.Source.StartActivity(
            "ServiceBus Publish",
            ActivityKind.Producer);

        if (activity is not null)
        {
            activity.SetTag("messaging.system", "azure_service_bus");
            activity.SetTag("messaging.destination", _sender.EntityPath);
            activity.SetTag("messaging.destination_kind", "topic");
            activity.SetTag("messaging.operation", "publish");
        }

        var serviceBusMessage = new ServiceBusMessage(
            BinaryData.FromObjectAsJson(message))
        {
            MessageId = Guid.NewGuid().ToString(),
            CorrelationId = message.CorrelationId,
            ContentType = "application/json"
        };

        // Inject OpenTelemetry trace context into message headers
        if (activity is not null)
        {
            Propagator.Inject(
                new PropagationContext(activity.Context, Baggage.Current),
                serviceBusMessage,
                static (msg, key, value) =>
                {
                    msg.ApplicationProperties[key] = value;
                });

            activity.SetTag("messaging.message_id", serviceBusMessage.MessageId);
        }

        try
        {
            await _sender.SendMessageAsync(serviceBusMessage, cancellationToken);

            _logger.LogInformation(
                "ServiceBus message published. MessageType={MessageType} MessageId={MessageId} CorrelationId={CorrelationId}",
                typeof(TPayload).Name,
                serviceBusMessage.MessageId,
                message.CorrelationId);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

            _logger.LogError(
                ex,
                "Failed to publish {MessageType}. CorrelationId={CorrelationId}",
                typeof(TPayload).Name,
                message.CorrelationId);

            throw;
        }
    }

    /// <inheritdoc />
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
