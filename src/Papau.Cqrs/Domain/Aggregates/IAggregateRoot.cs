using System.Collections.Generic;
using System.Threading.Tasks;

namespace Papau.Cqrs.Domain.Aggregates;

public interface IAggregateRoot
{
    IAggregateId Id { get; }
    int Version { get; }
    void ClearUncommittedChanges();
    Task ApplyChanges(IAsyncEnumerable<IEvent> changes);
    IReadOnlyList<IEvent> GetUncommittedChanges();
}