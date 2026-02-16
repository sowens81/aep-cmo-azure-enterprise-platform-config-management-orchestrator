using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Aep.Cmo.Event.Orchestrator.Domain.Models;

public class EventGridEvent<TPayload>
{
    [Required]
    public required string Id { get; set; }

    [Required]
    public required string Topic { get; set; }

    [Required]
    public required string Subject { get; set; }

    [Required]
    [JsonPropertyName("eventType")]
    public required string EventType { get; set; }

    [Required]
    [JsonPropertyName("data")]
    public required TPayload Data { get; set; }

    [Required]
    [JsonPropertyName("eventTime")]
    public required DateTime EventTime { get; set; }

    [Required]
    [JsonPropertyName("dataVersion")]
    public required string DataVersion { get; set; }

    [Required]
    [JsonPropertyName("metadataVersion")]
    public required string MetadataVersion { get; set; }
}
