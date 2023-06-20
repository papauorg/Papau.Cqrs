using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using EventStore.Client;

using Papau.Cqrs.Domain;
using Papau.Cqrs.Domain.Aggregates;
using Papau.Cqrs.Domain.Entities;

namespace Papau.Cqrs.EventStore;

public class EventStoreRepository<TAggregate>
    : AggregateRepository<TAggregate> where TAggregate : IAggregateRoot, new()
{
    public const int READ_PAGE_SIZE = 50;
    public const string AGGREGATE_CLR_TYPE_HEADER = "AggregateClrType";

    public EventStoreClient EventStoreClient { get; }
    public IEventSerializer EventSerializer { get; }

    public EventStoreRepository(
        IEventPublisher eventPublisher,
        EventStoreClient eventStoreClient,
        IEventSerializer eventSerializer
        )
        : base(eventPublisher)
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

    public override Task<TAggregate> GetById(IEntityId aggregateId)
    {
        return GetById(aggregateId, int.MaxValue);
    }

    protected async Task<TAggregate> GetById(IEntityId aggregateId, int version)
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

        return await BuildFromHistory(aggregateId, domainEvents, version).ConfigureAwait(false);
    }

    protected override async Task SaveInternal(IAggregateRoot aggregateRoot)
    {
        var events = await Save(aggregateRoot, null).ConfigureAwait(false);
        await PublishEndpoint.Publish(events).ConfigureAwait(false);
    }

    private static string GetStreamName(IEntityId id, Type type)
    {
        return $"{type.Name}-{id.ToString()}";
    }
}