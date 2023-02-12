using System;
using System.Collections.Generic;
using System.Text;
using EventStore.Client;
using Newtonsoft.Json;

using Papau.Cqrs.Domain;

namespace Papau.Cqrs.EventStore
{
    public class EventSerializer : IEventSerializer
    {
        public const string EVENT_CLR_TYPE_HEADER = "EventClrType";

        public JsonSerializerSettings SerializerSettings { get; }

        public EventSerializer(JsonSerializerSettings serializerSettings)
        {
            SerializerSettings = serializerSettings ?? throw new ArgumentNullException(nameof(serializerSettings));
        }

        public EventData ToEventData(IEvent @event, IDictionary<string, object> headers)
        {
            var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event, SerializerSettings));

            var eventHeaders = new Dictionary<string, object>(headers)
            {
                { EVENT_CLR_TYPE_HEADER, @event.GetType().AssemblyQualifiedName }
            };
            var metadata = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(eventHeaders, SerializerSettings));
            var typeName = @event.GetType().Name;

            return new EventData(Uuid.NewUuid(), typeName, data, metadata);
        }

        public IEvent DeserializeEvent(EventRecord rawEvent)
        {
            try
            {
                var headerString = Encoding.UTF8.GetString(rawEvent.Metadata.Span);
                var headers = JsonConvert.DeserializeObject<Dictionary<string, object>>(headerString);

                var typeName = headers[EVENT_CLR_TYPE_HEADER].ToString();
                var eventString = Encoding.UTF8.GetString(rawEvent.Data.Span);

                if (JsonConvert.DeserializeObject(eventString, Type.GetType(typeName)) is not IEvent @event)
                    throw new InvalidOperationException("Event is not of type IEvent");

                return @event;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Could not deserialize event '{rawEvent.EventId}' in stream '{rawEvent.EventStreamId}'", ex);
            }
        }
    }
}