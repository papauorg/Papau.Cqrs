using System.Threading.Tasks;

namespace Papau.Cqrs.Domain.Aggregates
{
    public interface IAggregateRepository<TAggregate> where TAggregate : IAggregateRoot
    {
        Task<TAggregate> GetById(IAggregateId aggregateId);
        Task Save(TAggregate aggregate);
    }
}