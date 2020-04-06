using System.Collections.Generic;
using EventStore.ClientAPI;
using Papau.Cqrs.Domain;

namespace Papau.Cqrs.EventStore
{
    public interface IEventSerializer
    {
         EventData ToEventData(IEvent @event, IDictionary<string, object> headers);
         IEvent DeserializeEvent(RecordedEvent rawEvent);
    }
}