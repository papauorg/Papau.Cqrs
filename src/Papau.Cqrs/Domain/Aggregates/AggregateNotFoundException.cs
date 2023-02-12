using System;

namespace Papau.Cqrs.Domain.Aggregates
{
    public class AggregateNotFoundException : Exception
    {
        public IAggregateId AggregateId { get; }
        public Type AggregateType { get; }

        public AggregateNotFoundException(IAggregateId aggregateId, Type aggregateType)
        {
            AggregateId = aggregateId ?? throw new ArgumentNullException(nameof(aggregateId));
            AggregateType = aggregateType ?? throw new ArgumentNullException(nameof(aggregateType));
        }
    }
}