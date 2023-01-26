using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Papau.Cqrs.Domain.Aggregates
{
    public interface IAggregateRepository
    {
        Task Save(IAggregateRoot aggregateRoot);

        Task<IAggregateRoot> GetById(Type aggregateType, IAggregateId aggregateId);

        Task<IEnumerable<IEvent>> GetAllEvents();
    }
}