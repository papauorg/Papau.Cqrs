using System;

namespace Papau.Cqrs.Domain.Aggregates;

public class AggregateVersionException : Exception
{
    public AggregateVersionException(IAggregateId aggregateId, Type aggregateType, int actualVersion, int expectedVersion)
    {
        AggregateId = aggregateId ?? throw new System.ArgumentNullException(nameof(aggregateId));
        AggregateType = aggregateType ?? throw new ArgumentNullException(nameof(aggregateType));
        ActualVersion = actualVersion;
        ExpectedVersion = expectedVersion;
    }

    public IAggregateId AggregateId { get; }
    public Type AggregateType { get; }
    public int ActualVersion { get; }
    public int ExpectedVersion { get; }
}