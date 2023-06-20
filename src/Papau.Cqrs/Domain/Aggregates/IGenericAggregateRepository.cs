using System.Threading.Tasks;

using Papau.Cqrs.Domain.Entities;

namespace Papau.Cqrs.Domain.Aggregates;

public interface IAggregateRepository<TAggregate> where TAggregate : IAggregateRoot
{
    Task<TAggregate> GetById(IEntityId aggregateId);
    Task Save(TAggregate aggregate);
}