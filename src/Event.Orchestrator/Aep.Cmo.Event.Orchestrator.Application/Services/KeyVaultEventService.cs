using Aep.Cmo.Event.Orchestrator.Application.Interfaces;
using Aep.Cmo.Event.Orchestrator.Domain.Enum;
using Aep.Cmo.Event.Orchestrator.Domain.Models;
using Aep.Cmo.Event.Orchestrator.Infrastructure.Interfaces;
using Aep.Cmo.Shared.Contracts.Enum;
using Aep.Cmo.Shared.Domain.Enums;
using Aep.Cmo.Shared.Domain.Results;
using Aep.Cmo.Shared.ServiceBus.Messaging;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Aep.Cmo.Event.Orchestrator.Application.Services;

public sealed class KeyVaultEventService : IKeyVaultEventService
{
    private static readonly ActivitySource ActivitySource =
        new("ConfigManagement.KeyVault.Event.Orchestrator.Application");

    private readonly ILogger<KeyVaultEventService> _logger;
    private readonly IHubKeyVaultSecretClient _hubKeyVaultSecretClient;
    private readonly IKeyVaultTopicPublisherClient _publisher;

    public KeyVaultEventService(
        ILogger<KeyVaultEventService> logger,
        IHubKeyVaultSecretClient hubKeyVaultSecretClient,
        IKeyVaultTopicPublisherClient keyVaultTopicPublisherClient)
    {
        _logger = logger;
        _hubKeyVaultSecretClient = hubKeyVaultSecretClient;
        _publisher = keyVaultTopicPublisherClient;
    }

    public async Task<Result> EventKeyVaultAsync(
        KeyVaultEvent message,
        string correlationId,
        CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity(
            "KeyVaultEventService.Event",
            ActivityKind.Internal);

        activity?.SetTag("event.id", message.Id);
        activity?.SetTag("event.type", message.EventType.ToString());
        activity?.SetTag("secret.name", message.Data.ObjectName);

        try
        {
            var secretName = message.Data.ObjectName;

            if (string.IsNullOrWhiteSpace(secretName))
            {
                return Result.Failure(
                    ResultStatus.ValidationError,
                    "Secret name is missing from event.");
            }

            // Only support UPSERT
            if (message.EventType != KeyVaultEventType.SecretNewVersionCreated)
            {
                _logger.LogInformation(
                    "Ignoring unsupported KeyVault event type {EventType} for secret {Secret}",
                    message.EventType,
                    secretName);

                return Result.Failure(
                    ResultStatus.Ignore,
                    $"Unsupported KeyVault event type {message.EventType}");
            }

            var hubSecret = await GetSecretWithTagsAsync(
                secretName,
                cancellationToken);

            if (hubSecret is null)
            {
                _logger.LogWarning(
                    "Hub secret {Secret} not found.",
                    secretName);

                return Result.Failure(
                    ResultStatus.DeadLetter,
                    $"Hub secret {secretName} not found.");
            }

            if (!ShouldSyncToSpoke(hubSecret))
            {
                _logger.LogInformation(
                    "Secret {Secret} does not have SyncToSpoke=true. Skipping sync.",
                    secretName);

                return Result.Success();
            }

            if (message.Data.Expiration.HasValue &&
                DateTimeOffset.FromUnixTimeSeconds(message.Data.Expiration.Value) < DateTimeOffset.UtcNow)
            {
                _logger.LogInformation(
                    "Secret {Secret} is expired. Skipping sync.",
                    secretName);

                return Result.Success();
            }

            var payload = new KeyVaultMessage(
                hubSecret.Id?.ToString() ?? message.Data.Id,
                secretName,
                SyncAction.Upsert);

            await _publisher.PublishAsync(
                payload,
                correlationId,
                cancellationToken);

            activity?.SetStatus(ActivityStatusCode.Ok);

            return Result.Success();
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

            _logger.LogError(
                ex,
                "Unhandled error processing KeyVault event for secret {Secret}",
                message.Data.ObjectName);

            return Result.Failure(
                ResultStatus.TransientFailure,
                "Unhandled infrastructure failure.");
        }
    }

    private Task<KeyVaultSecret?> GetSecretWithTagsAsync(
        string name,
        CancellationToken cancellationToken)
    {
        return _hubKeyVaultSecretClient.GetSecretAsync(
            name,
            cancellationToken);
    }

    private static bool ShouldSyncToSpoke(KeyVaultSecret secret)
    {
        if (secret.Properties.Tags is null)
            return false;

        if (!secret.Properties.Tags.TryGetValue("SyncToSpoke", out var value))
            return false;

        return string.Equals(
            value,
            "true",
            StringComparison.OrdinalIgnoreCase);
    }
}
