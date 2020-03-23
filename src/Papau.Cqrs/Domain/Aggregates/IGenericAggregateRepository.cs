using System.Threading.Tasks;

namespace Papau.Cqrs.Domain.Aggregates
{
    public interface IAggregateRepository<TAggregate> where TAggregate : AggregateRoot
    {
        Task<TAggregate> GetById(string aggregateId);
        Task Save(TAggregate aggregate);
    }
}