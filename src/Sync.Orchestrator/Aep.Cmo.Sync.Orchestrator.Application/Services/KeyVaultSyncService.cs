using Aep.Cmo.Shared.Contracts.Enum;
using Aep.Cmo.Shared.Domain.Enums;
using Aep.Cmo.Shared.Domain.Results;
using Aep.Cmo.Shared.KeyVault.Interfaces;
using Aep.Cmo.Shared.ServiceBus.Messaging;
using Aep.Cmo.Sync.Orchestrator.Application.Interfaces;
using Aep.Cmo.Sync.Orchestrator.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Aep.Cmo.Sync.Orchestrator.Application.Services;

public sealed class KeyVaultSyncService : IKeyVaultSyncService
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

    public async Task<Result> SyncKeyVaultAsync(
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
            return message.SyncAction switch
            {
                SyncAction.Upsert =>
                    await HandleUpsertAsync(message, cancellationToken),

                SyncAction.Delete =>
                    await HandleDeleteAsync(message, cancellationToken),

                _ => Result.Failure(
                        ResultStatus.Ignore,
                        $"Unsupported SyncAction {message.SyncAction}")
            };
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

            _logger.LogError(
                ex,
                "Unhandled error syncing KeyVault secret {Secret}",
                message.SecretName);

            return Result.Failure(
                ResultStatus.TransientFailure,
                "Unhandled infrastructure failure.");
        }
    }

    private async Task<Result> HandleUpsertAsync(
        KeyVaultMessage message,
        CancellationToken ct)
    {
        var hubSecret = await _hubKeyVault
            .GetSecretValueAsync(message.SecretName, ct);

        if (hubSecret is null || hubSecret.Value.Value is null)
        {
            return Result.Failure(
                ResultStatus.DeadLetter,
                $"Hub secret {message.SecretName} not found.");
        }

        var secretValue = hubSecret.Value.Value;

        if (!await _keyVault.SecretExistsAsync(message.SecretName, ct))
        {
            await _keyVault.CreateSecretAsync(
                message.SecretName,
                secretValue,
                ct);
        }
        else
        {
            await _keyVault.SetSecretAsync(
                message.SecretName,
                secretValue,
                ct);
        }

        return Result.Success();
    }

    private async Task<Result> HandleDeleteAsync(
        KeyVaultMessage message,
        CancellationToken ct)
    {
        if (!await _keyVault.SecretExistsAsync(message.SecretName, ct))
            return Result.Success(); 

        await _keyVault.DeleteSecretAsync(
            message.SecretName,
            ct);

        return Result.Success();
    }
}
