using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Papau.Cqrs.Domain.Aggregates;

public abstract class AggregateRepository<TAggregate>
    : IAggregateRepository, IAggregateRepository<TAggregate> where TAggregate : IAggregateRoot, new()
{
    public IEventPublisher PublishEndpoint { get; }

    public AggregateRepository(IEventPublisher publishEndpoint)
    {
        PublishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
    }

    protected async Task<TAggregate> BuildFromHistory(IAggregateId aggregateId, IAsyncEnumerable<IEvent> history, int expectedVersion)
    {
        var result = new TAggregate();

        await result.ApplyChanges(history).ConfigureAwait(false);

        if (result.Version == 0)
            throw new AggregateNotFoundException(aggregateId, typeof(TAggregate));

        if (expectedVersion < int.MaxValue && result.Version != expectedVersion)
            throw new AggregateVersionException(aggregateId, typeof(TAggregate), result.Version, expectedVersion);

        return result;
    }

    protected async Task<IEnumerable<IEvent>> CommitAndPublish(IAggregateId aggregateId, IEnumerable<IEvent> existingEvents, IAggregateRoot aggregate)
    {
        var uncommittedEvents = aggregate.GetUncommittedChanges();
        var versionBeforeChanges = aggregate.Version - uncommittedEvents.Count;

        var currentlySavedVersion = existingEvents.Count();

        if (versionBeforeChanges != currentlySavedVersion)
            throw new AggregateVersionException(aggregateId, typeof(TAggregate), versionBeforeChanges, currentlySavedVersion);

        aggregate.ClearUncommittedChanges();
        await PublishEndpoint.Publish(uncommittedEvents).ConfigureAwait(false);

        if (versionBeforeChanges == 0)
            return uncommittedEvents;
        else
            return existingEvents.Concat(uncommittedEvents);
    }

    public async Task Save(TAggregate aggregateRoot)
    {
        await SaveInternal(aggregateRoot).ConfigureAwait(false);
    }

    public async Task Save(IAggregateRoot aggregateRoot)
    {
        await SaveInternal(aggregateRoot).ConfigureAwait(false);
    }

    protected abstract Task SaveInternal(IAggregateRoot aggregateRoot);

    public abstract Task<TAggregate> GetById(IAggregateId aggregateId);

}