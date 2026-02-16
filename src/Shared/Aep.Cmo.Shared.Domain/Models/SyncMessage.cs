
using Aep.Cmo.Shared.ServiceBus.Models;
using System.ComponentModel.DataAnnotations;

namespace Aep.Cmo.Shared.Domain.Models;

public sealed class SyncMessage<TPayload> : BaseMessage<TPayload>
{
    [Required]
    public string EventGridId { get; init; } = default!;
}
