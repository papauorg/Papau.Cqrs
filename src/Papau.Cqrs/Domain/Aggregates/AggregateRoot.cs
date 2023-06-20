using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Papau.Cqrs.Domain.Entities;
namespace Papau.Cqrs.Domain.Aggregates;

/// <summary>
/// Aggregate root base class
/// </summary>
public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot 
    where TId : IEntityId
{
    private readonly List<IEvent> _changes;
    public int Version { get; private set; }
    IEntityId IAggregateRoot.Id => Id;

    protected AggregateRoot(TId id) : base(id)
    {
        _changes = new List<IEvent>();
        Version = 0;
    }

    public async Task LoadFromHistory(IAsyncEnumerable<IEvent> events)
    {
        await foreach (var @event in events.ConfigureAwait(false))
            ApplyChange(@event);
    }

    protected void RaiseEvent<TEvent>(Func<TEvent> createEvent) where TEvent : IEvent
    {
        var e = createEvent();
        _changes.Add(e);
        ApplyChange(e);
    }

    private void ApplyChange(IEvent @event)
    {
        Apply(@event);
        Version++;
    }

    public IReadOnlyList<IEvent> GetUncommittedChanges()
    {
        return _changes.AsReadOnly();
    }

    public void ClearUncommittedChanges()
    {
        _changes.Clear();
    }
    
    protected abstract void Apply(IEvent @event);
}