using System.Collections.Generic;

using EventStore.Client;

using Papau.Cqrs.Domain;

namespace Papau.Cqrs.EventStore;

public interface IEventSerializer
{
    EventData ToEventData(IEvent @event, IDictionary<string, object> headers);
    IEvent DeserializeEvent(EventRecord rawEvent);
}