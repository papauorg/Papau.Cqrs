
using System;

namespace Papau.Cqrs.Domain.Aggregates;

public abstract class AggregateId<TPrimitiveId> : IAggregateId, IEquatable<AggregateId<TPrimitiveId>>, IComparable<AggregateId<TPrimitiveId>> where TPrimitiveId : IComparable
{
    public TPrimitiveId Value { get; }

    public AggregateId(TPrimitiveId value)
    {
        Value = value;
    }

    public bool Equals(AggregateId<TPrimitiveId>? other) => Value.Equals(other!.Value);
    public int CompareTo(AggregateId<TPrimitiveId>? other) => Value.CompareTo(other!.Value);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value?.ToString() ?? string.Empty;
    public static bool operator ==(AggregateId<TPrimitiveId> a, AggregateId<TPrimitiveId> b) => a.CompareTo(b) == 0;
    public static bool operator !=(AggregateId<TPrimitiveId> a, AggregateId<TPrimitiveId> b) => !(a == b);

    public static implicit operator string(AggregateId<TPrimitiveId> id)
    {
        return id?.Value?.ToString() ?? string.Empty;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;

        return obj is AggregateId<TPrimitiveId> other && Equals(other);
    }
}
