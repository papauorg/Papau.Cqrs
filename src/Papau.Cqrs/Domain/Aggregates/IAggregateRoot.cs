using System.Collections.Generic;

namespace Papau.Cqrs.Domain.Aggregates
{
    public interface IAggregateRoot
    {
        IAggregateId Id { get; }
        int Version { get; }
        void ClearUncommittedChanges();
        void ApplyChanges(IEnumerable<IEvent> events);
        IReadOnlyList<IEvent> GetUncommittedChanges();
    }
}