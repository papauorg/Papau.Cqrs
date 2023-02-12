using System.Collections.Generic;
using System.Threading.Tasks;

using MassTransit;

using Papau.Cqrs.Domain;

namespace Papau.Cqrs.Masstransit;

public class MasstransitEventPublisher : IEventPublisher
{
    public IPublishEndpoint Endpoint { get; }

    public MasstransitEventPublisher(IPublishEndpoint endpoint)
    {
        Endpoint = endpoint ?? throw new System.ArgumentNullException(nameof(endpoint));
    }

    public async Task Publish(IEvent eventToPublish)
    {
        await Endpoint.Publish(eventToPublish, eventToPublish.GetType()).ConfigureAwait(false);
    }

    public async Task Publish(IEnumerable<IEvent> eventsToPublish)
    {
        foreach (var ev in eventsToPublish)
        {
            await Publish(ev).ConfigureAwait(false);
        }
    }
}