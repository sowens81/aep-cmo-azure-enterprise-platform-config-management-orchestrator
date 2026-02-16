using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Aep.Cmo.Event.Orchestrator.Domain.Models;

public sealed class KeyVaultSecretEventData
{
    [Required]
    public required string Id { get; init; }

    [Required]
    public required string VaultName { get; init; }

    [Required]
    public required string ObjectType { get; init; }

    [Required]
    public required string ObjectName { get; init; }

    [Required]
    public required string Version { get; init; }

    /// <summary>
    /// Not Before (Unix epoch seconds). Null if not set.
    /// </summary>
    [JsonPropertyName("NBF")]
    public long? NotBefore { get; init; }

    /// <summary>
    /// Expiration (Unix epoch seconds). Null if not set.
    /// </summary>
    [JsonPropertyName("EXP")]
    public long? Expiration { get; init; }
}
