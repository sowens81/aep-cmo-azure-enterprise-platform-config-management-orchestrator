using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Aep.Cmo.Event.Orchestrator.Domain.Models;

public class AppConfigurationEventData
{
    [Required]
    [MinLength(1)]
    [JsonPropertyName("key")]
    public required string Key { get; init; }

    [Required]
    [MinLength(1)]
    [JsonPropertyName("label")]
    public required string Label { get; init; }

    [Required]
    [MinLength(1)]
    [JsonPropertyName("etag")]
    public required string Etag { get; init; }

    [Required]
    [MinLength(1)]
    [JsonPropertyName("syncToken")]
    public required string SyncToken { get; init; }
}