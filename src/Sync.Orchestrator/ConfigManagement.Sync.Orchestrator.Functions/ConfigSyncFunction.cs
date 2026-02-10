using ConfigManagement.Shared.Domain.Models;
using ConfigManagement.Shared.ServiceBus.Models;
using ConfigManagement.Sync.Orchestrator.Application.Interfaces;
using ConfigManagement.Sync.Orchestrator.Application.Orchestration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace ConfigManagement.Sync.Orchestrator.Functions;

public sealed class ConfigSyncFunction
{
    private readonly IConfigSyncHandler _handler;
    private readonly IResultOrchestrator _resultOrchestrator;
    private readonly ILogger<ConfigSyncFunction> _logger;

    public ConfigSyncFunction(
        IConfigSyncHandler handler,
        IResultOrchestrator resultOrchestrator,
        ILogger<ConfigSyncFunction> logger
    )
    {
        _handler = handler;
        _resultOrchestrator = resultOrchestrator;
        _logger = logger;
    }

    [Function(nameof(ConfigSyncFunction))]
    public async Task RunAsync(
        [ServiceBusTrigger(
        "%SBUS_APP_CONFIG_TOPIC%",
        "%SBUS_APP_CONFIG_SUBSCRIPTION%",
        Connection = "ServiceBusConnection")]
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
