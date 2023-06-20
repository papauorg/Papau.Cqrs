using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Papau.Cqrs.Domain.Entities;

namespace Papau.Cqrs.Domain.Aggregates;

public abstract class AggregateRepository<TAggregate>
    : IAggregateRepository, IAggregateRepository<TAggregate> where TAggregate : IAggregateRoot, new()
{
    public IEventPublisher PublishEndpoint { get; }

    public AggregateRepository(IEventPublisher publishEndpoint)
    {
        PublishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
    }

    protected async Task<TAggregate> BuildFromHistory(IEntityId aggregateId, IAsyncEnumerable<IEvent> history, int expectedVersion)
    {
        var result = new TAggregate();

        await result.LoadFromHistory(history).ConfigureAwait(false);

        if (result.Version == 0)
            throw new AggregateNotFoundException(aggregateId, typeof(TAggregate));

        if (expectedVersion < int.MaxValue && result.Version != expectedVersion)
            throw new AggregateVersionException(aggregateId, typeof(TAggregate), result.Version, expectedVersion);

        return result;
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

    public abstract Task<TAggregate> GetById(IEntityId aggregateId);

}