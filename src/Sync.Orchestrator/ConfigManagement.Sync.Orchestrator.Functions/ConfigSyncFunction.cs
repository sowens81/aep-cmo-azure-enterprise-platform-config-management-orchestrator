using System.Diagnostics;
using ConfigManagement.Shared.Domain.Models;
using ConfigManagement.Shared.ServiceBus.Models;
using ConfigManagement.Sync.Orchestrator.Application.Interfaces;
using ConfigManagement.Sync.Orchestrator.Application.Orchestration;
using ConfigurationSyncOrchestrator.Domain.Factories;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ConfigManagement.Sync.Orchestrator.Functions;

public sealed class ConfigSyncFunction
{
    private readonly ConfigFactory _config;
    private readonly IConfigSyncHandler _handler;
    private readonly ISyncResultOrchestrator _resultOrchestrator;
    private readonly ILogger<ConfigSyncFunction> _logger;

    public ConfigSyncFunction(
        ConfigFactory config,
        IConfigSyncHandler handler,
        ISyncResultOrchestrator resultOrchestrator,
        ILogger<ConfigSyncFunction> logger
    )
    {
        _config = config;
        _handler = handler;
        _resultOrchestrator = resultOrchestrator;
        _logger = logger;
    }

    [Function(nameof(ConfigSyncFunction))]
    public async Task RunAsync(
        [ServiceBusTrigger(
            "%SBUS_EVENT_TOPIC%",
            "%SBUS_EVENT_TOPIC_SUBSCRIPTION%",
            Connection = "ServiceBus__FullyQualifiedNamespace"
        )]
            EventMessage<ConfigSyncMessage> message,
        CancellationToken ct
    )
    {
        var traceId = string.IsNullOrWhiteSpace(message.TraceId)
            ? Activity.Current?.TraceId.ToString() ?? string.Empty
            : message.TraceId;

        var spanId = !string.IsNullOrWhiteSpace(message.SpanId)
            ? message.SpanId
            : Activity.Current?.SpanId.ToString();

        using (
            _logger.BeginScope(
                new Dictionary<string, object?>
                {
                    ["EventType"] = message.EventType,
                    ["CorrelationId"] = message.CorrelationId,
                    ["TraceId"] = traceId,
                    ["SpanId"] = spanId,
                    ["EnvironmentName"] = message.EnvironmentName,
                    ["Source"] = message.Source,
                }
            )
        )
        {
            _logger.LogInformation("Starting sync message processing");

            var result = await _handler.HandleAsync(message.Payload, ct);

            await _resultOrchestrator.HandleResultAsync(message, result, ct);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Sync message processing failed");
                throw new InvalidOperationException(result.Error);
            }

            _logger.LogInformation("Sync message processing completed");
        }
    }
}
