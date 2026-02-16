using Aep.Cmo.Event.Orchestrator.Domain.Converters;
using Aep.Cmo.Event.Orchestrator.Domain.Enum;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Aep.Cmo.Event.Orchestrator.Domain.Models;

public class AppConfigurationEvent : EventGridEvent
{
    [Required]
    [JsonPropertyName("data")]
    public required AppConfigurationEventData Data { get; init; }

    [Required]
    [JsonPropertyName("eventType")]
    [JsonConverter(typeof(AppConfigurationEventTypeJsonConverter))]
    public required AppConfigurationEventType EventType { get; init; }

}
