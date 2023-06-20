using System.Collections.Generic;
using System.Threading.Tasks;

using Papau.Cqrs.Domain.Entities;

namespace Papau.Cqrs.Domain.Aggregates;

public interface IAggregateRoot
{
    IEntityId Id { get; }
    int Version { get; }
    void ClearUncommittedChanges();
    Task LoadFromHistory(IAsyncEnumerable<IEvent> changes);
    IReadOnlyList<IEvent> GetUncommittedChanges();
}