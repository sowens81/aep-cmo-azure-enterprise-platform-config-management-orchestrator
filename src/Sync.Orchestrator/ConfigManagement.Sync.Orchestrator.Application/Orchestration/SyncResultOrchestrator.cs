using ConfigManagement.Shared.Domain;
using ConfigManagement.Shared.Domain.Models;
using ConfigManagement.Shared.Domain.Results;
using ConfigManagement.Shared.ServiceBus;
using ConfigManagement.Shared.ServiceBus.Models;
using ConfigManagement.Sync.Orchestrator.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace ConfigManagement.Sync.Orchestrator.Application.Orchestration;

public sealed class SyncResultOrchestrator : ISyncResultOrchestrator
{
    private const string SuccessStatus = "SUCCESS";
    private const string FailedStatus = "FAILED";

    private readonly IResultPublisher _resultPublisher;
    private readonly IServiceMetadata _serviceMetadata;
    private readonly ILogger<SyncResultOrchestrator> _logger;

    public SyncResultOrchestrator(
        IResultPublisher resultPublisher,
        IServiceMetadata serviceMetadata,
        ILogger<SyncResultOrchestrator> logger
    )
    {
        _resultPublisher = resultPublisher;
        _serviceMetadata = serviceMetadata;
        _logger = logger;
    }

    public async Task HandleResultAsync(
        EventMessage<ConfigSyncMessage> sourceMessage,
        Result<Unit> result,
        CancellationToken cancellationToken
    )
    {
        var resultMessage = new ResultMessage<ConfigSyncMessage>
        {
            EventType = sourceMessage.EventType,
            Source = sourceMessage.Source,

            Organisation = _serviceMetadata.Organisation,
            Region = _serviceMetadata.Region,
            EnvironmentTier = _serviceMetadata.EnvironmentTier,
            EnvironmentName = sourceMessage.EnvironmentName,
            ServiceName = _serviceMetadata.ServiceName,

            CorrelationId = sourceMessage.CorrelationId,
            TraceId = string.IsNullOrWhiteSpace(sourceMessage.TraceId)
                ? Activity.Current?.TraceId.ToString() ?? string.Empty
                : sourceMessage.TraceId,
            SpanId = Activity.Current?.SpanId.ToString() ?? sourceMessage.SpanId,

            Status = result.IsSuccess ? SuccessStatus : FailedStatus,
            Message = result.IsSuccess ? null : result.Error,

            Payload = sourceMessage.Payload,
            TimestampUtc = DateTimeOffset.UtcNow,
        };

        _logger.LogInformation(
            "Publishing sync result. EventType={EventType}, CorrelationId={CorrelationId}, TraceId={TraceId}, SpanId={SpanId}, Status={Status}",
            resultMessage.EventType,
            resultMessage.CorrelationId,
            resultMessage.TraceId,
            resultMessage.SpanId,
            resultMessage.Status
        );

        await _resultPublisher.PublishAsync(resultMessage, cancellationToken);
    }
}
