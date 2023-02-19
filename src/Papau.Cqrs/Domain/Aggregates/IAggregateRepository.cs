using System.Threading.Tasks;

namespace Papau.Cqrs.Domain.Aggregates;

public interface IAggregateRepository
{
    Task Save(IAggregateRoot aggregateRoot);
}