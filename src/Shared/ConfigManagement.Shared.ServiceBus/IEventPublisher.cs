using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConfigManagement.Shared.ServiceBus.Models;

namespace ConfigManagement.Shared.ServiceBus;

public interface IEventPublisher
{
    Task PublishAsync<TPayload>(
        EventMessage<TPayload> message,
        CancellationToken cancellationToken);
}
