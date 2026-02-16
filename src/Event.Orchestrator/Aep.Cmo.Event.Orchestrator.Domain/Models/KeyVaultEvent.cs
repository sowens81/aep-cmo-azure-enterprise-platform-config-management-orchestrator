using Aep.Cmo.Event.Orchestrator.Domain.Converters;
using Aep.Cmo.Event.Orchestrator.Domain.Enum;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Aep.Cmo.Event.Orchestrator.Domain.Models;

public class KeyVaultEvent : EventGridEvent
{
    [Required]
    [JsonPropertyName("data")]
    public required KeyVaultSecretEventData Data { get; init; }

    [Required]
    [JsonPropertyName("eventType")]
    [JsonConverter(typeof(KeyVaultEventTypeJsonConverter))]
    public required KeyVaultEventType EventType { get; init; }

}
