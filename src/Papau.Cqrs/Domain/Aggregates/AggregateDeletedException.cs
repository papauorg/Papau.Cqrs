using System;

namespace Papau.Cqrs.Domain.Aggregates
{
    public class AggregateDeletedException : Exception
    {
        public AggregateDeletedException(String aggregateId, Type aggregateType)
        {
            AggregateId = aggregateId ?? throw new ArgumentNullException(nameof(aggregateId));
            AggregateType = aggregateType ?? throw new ArgumentNullException(nameof(aggregateType));
        }

        public string AggregateId { get; }
        public Type AggregateType { get; }
    }
}