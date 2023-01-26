using System;
using System.Linq;
using System.Collections.Generic;
using EventStore.ClientAPI;
using System.Threading.Tasks;
using Papau.Cqrs.Domain;
using Papau.Cqrs.Domain.Aggregates;

namespace Papau.Cqrs.EventStore
{
    public class EventStoreRepository<TAggregate> 
        : AggregateRepository<TAggregate> where TAggregate : IAggregateRoot
    {
        public const int READ_PAGE_SIZE = 50;
        public const string AGGREGATE_CLR_TYPE_HEADER = "AggregateClrType";

        public IEventStoreConnection EventStoreConnection { get; }
        public IEventSerializer EventSerializer { get; }

        public EventStoreRepository(
            IAggregateFactory aggregateFactory,
            IEventPublisher eventPublisher,
            IEventStoreConnection eventStoreConnection,
            IEventSerializer eventSerializer
            )
            : base(aggregateFactory, eventPublisher)
        {
            EventStoreConnection = eventStoreConnection ?? throw new ArgumentNullException(nameof(eventStoreConnection));
            EventSerializer = eventSerializer ?? throw new ArgumentNullException(nameof(eventSerializer));
        }

        protected async Task<IReadOnlyList<IEvent>> Save(IAggregateRoot aggregate, string streamName, Action<IDictionary<string, object>> updateHeaders)
        {
            var commitHeaders = new Dictionary<string, object>
            {
                { AGGREGATE_CLR_TYPE_HEADER, aggregate.GetType().AssemblyQualifiedName }
            };

            updateHeaders?.Invoke(commitHeaders);

            var newEvents = new List<IEvent>(aggregate.GetUncommittedChanges()).AsReadOnly();
            var originalVersion = aggregate.Version - newEvents.Count -1; // -1 because the stream beginns at version 0
            var expectedVersion = originalVersion < 0 ? ExpectedVersion.NoStream : originalVersion;
            var eventsToSave = newEvents.Select(e => EventSerializer.ToEventData(e, commitHeaders)).ToList();

            await EventStoreConnection.AppendToStreamAsync(streamName, expectedVersion, eventsToSave);

            aggregate.ClearUncommittedChanges();

            return newEvents;
        }

        protected async Task<TAggregate> GetById(IAggregateId streamName, int version)
        {
            if (version <= 0)
                throw new InvalidOperationException("Cannot get version <= 0");

            long sliceStart = StreamPosition.Start;
            StreamEventsSlice currentSlice;
            IAggregateRoot aggregate = null;
            do
            {
                var sliceCount = sliceStart + READ_PAGE_SIZE <= version
                                    ? READ_PAGE_SIZE
                                    : (int)(version - sliceStart + 1);

                currentSlice = await EventStoreConnection.ReadStreamEventsForwardAsync(streamName.ToString(), sliceStart, sliceCount, false);

                if (currentSlice.Status == SliceReadStatus.StreamNotFound)
                    throw new AggregateNotFoundException(streamName.ToString(), typeof(TAggregate));

                if (currentSlice.Status == SliceReadStatus.StreamDeleted)
                    throw new AggregateDeletedException(streamName.ToString(), typeof(TAggregate));

                sliceStart = currentSlice.NextEventNumber;

                var domainEvents = currentSlice.Events.Select(e => EventSerializer.DeserializeEvent(e.OriginalEvent)).ToList();
                if (aggregate == null)
                    aggregate = AggregateFactory.CreateAggregate<TAggregate>();
                else 
                    await ApplyChangesToAggregate(aggregate, domainEvents);
                
            } while (version >= currentSlice.NextEventNumber && !currentSlice.IsEndOfStream);

            if (version < Int32.MaxValue && aggregate.Version != version)
                throw new AggregateVersionException(streamName.ToString(), typeof(TAggregate), aggregate.Version, version);                

            return (TAggregate)aggregate;
        }

        protected override async Task SaveInternal(IAggregateRoot aggregateRoot)
        {
            var streamName = aggregateRoot.Id;
            var events = await Save(aggregateRoot, streamName.ToString(), null);
            await PublishEndpoint.Publish(events);
        }

        public override Task<IEnumerable<IEvent>> GetAllEvents()
        {
            throw new NotSupportedException();
        }

        public override async Task<IAggregateRoot> GetById(Type aggregateType, IAggregateId aggregateId)
        {
            return await GetById(aggregateId, int.MaxValue);
        }
    }
}