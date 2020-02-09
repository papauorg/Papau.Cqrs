using System;
using System.Collections.Generic;

namespace Papau.Cqrs.Domain.Aggregates
{
    /// Factory to create aggregates with data.
    public interface IAggregateFactory
    {
        /// Create and load with the given eventdata
        AggregateRoot CreateAggregate(Type aggregateType);

        /// Apply additional events to the aggregate
        TAggregate CreateAggregate<TAggregate>() where TAggregate : AggregateRoot;
    }
}