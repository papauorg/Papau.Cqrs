using System;

using Papau.Cqrs.Domain.Entities;

namespace Papau.Cqrs.Domain.Aggregates;

public class AggregateNotFoundException : Exception
{
    public IEntityId AggregateId { get; }
    public Type AggregateType { get; }

    public AggregateNotFoundException(IEntityId aggregateId, Type aggregateType)
    {
        AggregateId = aggregateId ?? throw new ArgumentNullException(nameof(aggregateId));
        AggregateType = aggregateType ?? throw new ArgumentNullException(nameof(aggregateType));
    }
}