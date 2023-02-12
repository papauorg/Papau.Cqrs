using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Papau.Cqrs.Domain.Aggregates;

/// <summary>
/// Aggregate root base class
/// </summary>
public abstract class AggregateRoot<TId> : IAggregateRoot where TId : IAggregateId
{
    private readonly List<IEvent> _changes;
    private readonly Dictionary<Type, Action<IEvent>> _eventConsumerMethods;
    public int Version { get; private set; }
    public TId Id { get; protected set; }

    IAggregateId IAggregateRoot.Id => this.Id;

    public AggregateRoot(TId id)
    {
        _changes = new List<IEvent>();
        _eventConsumerMethods = new Dictionary<Type, Action<IEvent>>();
        Version = 0;
        Id = id;
    }

    protected void Handle<TEvent>(Action<TEvent> handler) where TEvent : IEvent
    {
        if (handler == null)
            throw new ArgumentNullException(nameof(handler));

        _eventConsumerMethods.Add(typeof(TEvent), e => handler((TEvent)e));
    }

    public async Task ApplyChanges(IAsyncEnumerable<IEvent> events)
    {
        await foreach (var @event in events.ConfigureAwait(false))
            ApplyChange(@event, false);
    }

    protected void ApplyChange(IEvent @event)
    {
        ApplyChange(@event, isNew: true);
    }

    private void ApplyChange(IEvent @event, bool isNew)
    {
        var eventHandlingMethod = _eventConsumerMethods
            .Where(t => t.Key.IsAssignableFrom(@event.GetType()))
            .Select(a => a.Value)
            .Single();

        eventHandlingMethod(@event);
        if (isNew)
            _changes.Add(@event);
        Version++;
    }

    public IReadOnlyList<IEvent> GetUncommittedChanges()
    {
        return new List<IEvent>(_changes).AsReadOnly();
    }

    public void ClearUncommittedChanges()
    {
        _changes.Clear();
    }
}