using System.ComponentModel.DataAnnotations;

namespace Aep.Cmo.Event.Orchestrator.Domain.Models;

public sealed class AppConfigEventData
{
    [Required]
    public required string Key { get; init; }
    public string? Label { get; init; }
    [Required] 
    public required string Etag { get; init; }
    [Required]
    public required string SyncToken { get; init; }
}
