using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using EventStore.Client;

using Papau.Cqrs.Domain;
using Papau.Cqrs.Domain.Aggregates;

namespace Papau.Cqrs.EventStore;

public class EventStoreRepository<TAggregate>
    : AggregateRepository<TAggregate> where TAggregate : IAggregateRoot
{
    public const int READ_PAGE_SIZE = 50;
    public const string AGGREGATE_CLR_TYPE_HEADER = "AggregateClrType";

    public EventStoreClient EventStoreClient { get; }
    public IEventSerializer EventSerializer { get; }

    public EventStoreRepository(
        IAggregateFactory aggregateFactory,
        IEventPublisher eventPublisher,
        EventStoreClient eventStoreClient,
        IEventSerializer eventSerializer
        )
        : base(aggregateFactory, eventPublisher)
    {
        EventStoreClient = eventStoreClient ?? throw new ArgumentNullException(nameof(eventStoreClient));
        EventSerializer = eventSerializer ?? throw new ArgumentNullException(nameof(eventSerializer));
    }

    protected async Task<IReadOnlyList<IEvent>> Save(IAggregateRoot aggregate, Action<IDictionary<string, object>> updateHeaders)
    {
        var commitHeaders = new Dictionary<string, object>
        {
            { AGGREGATE_CLR_TYPE_HEADER, aggregate.GetType().AssemblyQualifiedName }
        };

        updateHeaders?.Invoke(commitHeaders);

        var streamName = GetStreamName(aggregate.Id, typeof(TAggregate));

        var newEvents = new List<IEvent>(aggregate.GetUncommittedChanges()).AsReadOnly();
        var originalVersion = aggregate.Version - newEvents.Count - 1; // -1 because the stream beginns at version 0
        var expectedVersion = originalVersion < 0 ? StreamRevision.None : StreamRevision.FromInt64(originalVersion);
        var eventsToSave = newEvents.Select(e => EventSerializer.ToEventData(e, commitHeaders)).ToList();

        await EventStoreClient.AppendToStreamAsync(streamName, expectedVersion, eventsToSave).ConfigureAwait(false);

        aggregate.ClearUncommittedChanges();

        return newEvents;
    }


    protected async Task<TAggregate> GetById(IAggregateId aggregateId, int version)
    {
        if (version <= 0)
            throw new InvalidOperationException("Cannot get version <= 0");

        var events = EventStoreClient.ReadStreamAsync(
            Direction.Forwards,
            GetStreamName(aggregateId, typeof(TAggregate)),
            StreamPosition.Start,
            resolveLinkTos: false
        );

        if (await events.ReadState.ConfigureAwait(false) == ReadState.StreamNotFound)
            throw new AggregateNotFoundException(aggregateId, typeof(TAggregate));

        var domainEvents = events
            .Take(version)
            .Select(e => EventSerializer.DeserializeEvent(e.OriginalEvent));

        var aggregate = await BuildFromHistory(typeof(TAggregate), aggregateId, domainEvents, version).ConfigureAwait(false);

        return (TAggregate)aggregate;
    }

    protected override async Task SaveInternal(IAggregateRoot aggregateRoot)
    {
        var events = await Save(aggregateRoot, null).ConfigureAwait(false);
        await PublishEndpoint.Publish(events).ConfigureAwait(false);
    }

    private static string GetStreamName(IAggregateId id, Type type)
    {
        return $"{type.Name}-{id.ToString()}";
    }

    public override async Task<IAggregateRoot> GetById(Type aggregateType, IAggregateId aggregateId)
    {
        return await GetById(aggregateId, int.MaxValue).ConfigureAwait(false);
    }
}