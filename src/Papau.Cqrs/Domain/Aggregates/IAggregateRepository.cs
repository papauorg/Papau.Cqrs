using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Papau.Cqrs.Domain.Aggregates
{
    public interface IAggregateRepository
    {
        Task Save(AggregateRoot aggregateRoot, string aggregateId);

        Task<AggregateRoot> GetById(Type aggregateType, string aggregateId);

        Task<IEnumerable<IEvent>> GetAllEvents();
    }
}