using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Papau.Cqrs.Domain.Aggregates
{
    public interface IAggregateRepository
    {
        Task Save(AggregateRoot aggregateRoot);

        Task<AggregateRoot> GetById(Type aggregateType, string aggregateId);

        Task<IEnumerable<IEvent>> GetAllEvents();
    }
}