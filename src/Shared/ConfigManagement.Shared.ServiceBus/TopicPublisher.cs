using Azure.Messaging.ServiceBus;
using ConfigManagement.Shared.ServiceBus.Interfaces;
using ConfigManagement.Shared.ServiceBus.Models;
using ConfigManagement.Shared.ServiceBus.OpenTelemetry;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using System.Diagnostics;

namespace ConfigManagement.Shared.ServiceBus;

/// <summary>
/// Publishes strongly-typed messages to an Azure Service Bus topic.
/// </summary>
/// <typeparam name="TMessage">
/// The message type being published. Must inherit from <see cref="BaseMessage{TPayload}"/>.
/// </typeparam>
/// <typeparam name="TPayload">
/// The payload type carried inside the message.
/// </typeparam>
/// <remarks>
/// This publisher automatically creates a distributed tracing span
/// (<see cref="ActivityKind.Producer"/>) for each publish operation.
/// If a parent <see cref="Activity"/> exists, the span will be linked
/// as a child, enabling full end-to-end trace correlation across services.
/// </remarks>
public class TopicPublisher<TMessage, TPayload> :
    ITopicPublisher<TMessage>,
    IAsyncDisposable
    where TMessage : BaseMessage<TPayload>
{
    private static readonly TextMapPropagator Propagator =
        Propagators.DefaultTextMapPropagator;
    private readonly ServiceBusSender _sender;
    private readonly ILogger<TopicPublisher<TMessage, TPayload>> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TopicPublisher{TMessage, TPayload}"/> class.
    /// </summary>
    /// <param name="topic">
    /// The topic configuration containing the topic name.
    /// </param>
    /// <param name="options">
    /// The Service Bus configuration options including the endpoint.
    /// </param>
    /// <param name="credentialFactory">
    /// A factory used to create the appropriate Azure credentials.
    /// </param>
    /// <param name="logger">
    /// The logger instance used for structured logging.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when required configuration values are missing.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when required dependencies are null.
    /// </exception>
    protected TopicPublisher(
        IServiceBusTopicOptions topic,
        IServiceBusOptions options,
        IServiceBusCredentialFactory credentialFactory,
        ILogger<TopicPublisher<TMessage, TPayload>> logger)
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

    /// <summary>
    /// Publishes a message to the configured Service Bus topic.
    /// </summary>
    /// <param name="message">
    /// The strongly-typed message instance to publish.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous publish operation.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="message"/> is null.
    /// </exception>
    /// <remarks>
    /// This method:
    /// <list type="bullet">
    /// <item>
    /// <description>Creates a producer span using <see cref="Telemetry.Source"/>.</description>
    /// </item>
    /// <item>
    /// <description>Serializes the message as JSON.</description>
    /// </item>
    /// <item>
    /// <description>Publishes it to Azure Service Bus.</description>
    /// </item>
    /// <item>
    /// <description>Logs structured success or failure details.</description>
    /// </item>
    /// </list>
    /// </remarks>
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
                MessageId = message.CorrelationId,
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

    /// <summary>
    /// Asynchronously releases the underlying <see cref="ServiceBusSender"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="ValueTask"/> representing the asynchronous dispose operation.
    /// </returns>
    /// <remarks>
    /// Any exceptions during disposal are logged as warnings and suppressed.
    /// </remarks>
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