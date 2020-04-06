using System;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Papau.Cqrs.Domain;

namespace Papau.Cqrs.EventStore
{
    public abstract class EventStoreReadingReadModel
    {
        private long? _subscribeFromEvent = StreamCheckpoint.StreamStart;
        public IEventStoreConnection EventStoreConnection { get; }
        public IEventSerializer EventSerializer { get; }

        public EventStoreReadingReadModel(
            IEventStoreConnection eventStoreConnection,
            IEventSerializer eventSerializer)
        {
            EventStoreConnection = eventStoreConnection ?? throw new System.ArgumentNullException(nameof(eventStoreConnection));
            EventSerializer = eventSerializer ?? throw new ArgumentNullException(nameof(eventSerializer));
        }

        protected abstract string GetStreamName();

        public async Task RebuildModel()
        {
            StreamEventsSlice currentSlice;
            long sliceStart = StreamPosition.Start;
            do
            {
                currentSlice = await EventStoreConnection.ReadStreamEventsForwardAsync(
                    GetStreamName(),
                    sliceStart,
                    50,
                    true
                );

                if (!_subscribeFromEvent.HasValue && currentSlice.LastEventNumber > -1)
                {
                    _subscribeFromEvent = currentSlice.LastEventNumber;
                }
                else if (_subscribeFromEvent.HasValue)
                {
                    _subscribeFromEvent = Math.Max(_subscribeFromEvent.Value, currentSlice.LastEventNumber);
                }
                sliceStart = currentSlice.NextEventNumber;

                foreach(var e in currentSlice.Events)
                    await DispatchEvents(e);
                
            } while (!currentSlice.IsEndOfStream);
        }

        public Task SubscribeToEvents()
        {
            EventStoreConnection.SubscribeToStreamFrom(
                GetStreamName(), 
                _subscribeFromEvent, 
                CatchUpSubscriptionSettings.Default, 
                (_, e) => DispatchEvents(e));
            
            return Task.CompletedTask;
        }

        private async Task DispatchEvents(ResolvedEvent @event)
        {
            if (!@event.Event.IsJson)
                throw new InvalidOperationException($"The event '{@event.OriginalEventNumber}' in stream '{@event.OriginalStreamId}' can't be dispatched in the read model because it is not a json event.");

            var eventToDispatch = EventSerializer.DeserializeEvent(@event.Event);
            if (eventToDispatch == null)
                throw new InvalidOperationException($"The event '{@event.OriginalEventNumber}' in stream '{@event.OriginalStreamId}' can't be processed in the read model because it couldn't be deserialized.");

            await Consume(eventToDispatch);
        }

        protected abstract Task Consume(IEvent e);
    }
}