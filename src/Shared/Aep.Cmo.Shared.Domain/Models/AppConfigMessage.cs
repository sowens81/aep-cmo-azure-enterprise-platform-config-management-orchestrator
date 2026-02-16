using Aep.Cmo.Shared.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace Aep.Cmo.Shared.Domain.Models;

public sealed class AppConfigMessage
{
    
    [Required]
    public required string ConfigKeyName { get; init; }

    [Required]
    public required ConfigSyncMessageType Type { get; init; }
    
    [Required]
    public required SyncAction SyncAction { get; init; }
}
