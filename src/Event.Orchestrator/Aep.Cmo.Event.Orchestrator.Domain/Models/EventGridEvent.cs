using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Aep.Cmo.Event.Orchestrator.Domain.Models;

public class EventGridEvent
{
    [Required]
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [Required]
    [JsonPropertyName("topic")]
    public required string Topic { get; init; }

    [Required]
    [JsonPropertyName("subject")]
    public required string Subject { get; init; }

    [Required]
    [JsonPropertyName("dataVersion")]
    public required string DataVersion { get; init; }

    [Required]
    [JsonPropertyName("metadataVersion")]
    public required string MetadataVersion { get; init; }

    [Required]
    [JsonPropertyName("eventTime")]
    public required DateTimeOffset EventTime { get; init; }
}