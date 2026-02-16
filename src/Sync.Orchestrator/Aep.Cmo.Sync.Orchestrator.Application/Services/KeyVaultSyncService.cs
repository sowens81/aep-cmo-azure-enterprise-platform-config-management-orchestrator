using Aep.Cmo.Shared.Domain;
using Aep.Cmo.Shared.Domain.Enum;
using Aep.Cmo.Shared.Domain.Models;
using Aep.Cmo.Shared.Domain.Results;
using Aep.Cmo.Shared.KeyVault.Interfaces;
using Aep.Cmo.Sync.Orchestrator.Application.Interfaces;
using Aep.Cmo.Sync.Orchestrator.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Aep.Cmo.Sync.Orchestrator.Application.Services;

public class KeyVaultSyncService : IKeyVaultSyncService
{
    private static readonly ActivitySource ActivitySource =
        new("ConfigManagement.KeyVault.Sync.Orchestrator.Application");

    private readonly ILogger<KeyVaultSyncService> _logger;
    private readonly IHubKeyVaultSecretClient _hubKeyVault;
    private readonly IKeyVaultSecretClient _keyVault;

    public KeyVaultSyncService(
        ILogger<KeyVaultSyncService> logger,
        IHubKeyVaultSecretClient hubKeyVault,
        IKeyVaultSecretClient keyVault)
    {
        _logger = logger;
        _hubKeyVault = hubKeyVault;
        _keyVault = keyVault;
    }

    public async Task<Result<Unit>> SyncKeyVaultAsync(
        KeyVaultMessage message,
        CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity(
            "KeyVaultSyncService.Sync",
            ActivityKind.Internal);

        activity?.SetTag("secret.name", message.SecretName);
        activity?.SetTag("secret.id", message.SecretId);
        activity?.SetTag("sync.action", message.SyncAction.ToString());

        try
        {
            if (message.SyncAction == SyncAction.Upsert)
                return await HandleUpsertAsync(message, cancellationToken);

            if (message.SyncAction == SyncAction.Delete)
                return await HandleDeleteAsync(message, cancellationToken);

            _logger.LogWarning(
                "Unsupported SyncAction {Action} for secret {Secret}",
                message.SyncAction,
                message.SecretName);

            return Result<Unit>.Failure(
                $"Unsupported SyncAction {message.SyncAction}");
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

            _logger.LogError(
                ex,
                "Unhandled error syncing KeyVault secret {Secret}",
                message.SecretName);

            throw; // Allow Service Bus retry
        }
    }

    // ------------------------------------------------------------
    // UPSERT
    // ------------------------------------------------------------

    private async Task<Result<Unit>> HandleUpsertAsync(
        KeyVaultMessage message,
        CancellationToken ct)
    {
        using var activity = ActivitySource.StartActivity(
            "KeyVaultSyncService.Upsert",
            ActivityKind.Internal);

        activity?.SetTag("secret.name", message.SecretName);

        _logger.LogInformation(
            "Starting KeyVault Upsert for secret {Secret}",
            message.SecretName);

        var hubSecret = await _hubKeyVault
            .GetSecretValueAsync(message.SecretName, ct);

        if (hubSecret is null || hubSecret.Value.Value is null)
        {
            _logger.LogWarning(
                "Hub secret {Secret} not found",
                message.SecretName);

            activity?.SetTag("sync.result", "hub_not_found");

            return Result<Unit>.Failure(
                $"Hub secret {message.SecretName} not found.");
        }

        var secretValue = hubSecret.Value.Value;

        var exists = await _keyVault
            .SecretExistsAsync(message.SecretName, ct);

        if (!exists)
        {
            _logger.LogInformation(
                "Creating secret {Secret} in target vault",
                message.SecretName);

            await _keyVault.CreateSecretAsync(
                message.SecretName,
                secretValue,
                ct);

            activity?.SetTag("sync.operation", "create");
        }
        else
        {
            _logger.LogInformation(
                "Updating secret {Secret} in target vault",
                message.SecretName);

            await _keyVault.SetSecretAsync(
                message.SecretName,
                secretValue,
                ct);

            activity?.SetTag("sync.operation", "update");
        }

        activity?.SetTag("sync.result", "success");
        activity?.SetStatus(ActivityStatusCode.Ok);

        _logger.LogInformation(
            "Successfully synced KeyVault secret {Secret}",
            message.SecretName);

        return Result<Unit>.Success(Unit.Value);
    }

    // ------------------------------------------------------------
    // DELETE
    // ------------------------------------------------------------

    private async Task<Result<Unit>> HandleDeleteAsync(
        KeyVaultMessage message,
        CancellationToken ct)
    {
        using var activity = ActivitySource.StartActivity(
            "KeyVaultSyncService.Delete",
            ActivityKind.Internal);

        activity?.SetTag("secret.name", message.SecretName);

        _logger.LogInformation(
            "Starting KeyVault Delete for secret {Secret}",
            message.SecretName);

        var exists = await _keyVault
            .SecretExistsAsync(message.SecretName, ct);

        if (!exists)
        {
            _logger.LogInformation(
                "Secret {Secret} does not exist. Delete treated as idempotent.",
                message.SecretName);

            activity?.SetTag("sync.result", "not_found");
            activity?.SetStatus(ActivityStatusCode.Ok);

            return Result<Unit>.Success(Unit.Value);
        }

        await _keyVault.DeleteSecretAsync(
            message.SecretName,
            ct);

        activity?.SetTag("sync.result", "deleted");
        activity?.SetStatus(ActivityStatusCode.Ok);

        _logger.LogInformation(
            "Deleted KeyVault secret {Secret}",
            message.SecretName);

        return Result<Unit>.Success(Unit.Value);
    }
}
