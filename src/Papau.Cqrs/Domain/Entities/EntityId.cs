namespace Papau.Cqrs.Domain.Entities;

using System;

public abstract record class EntityId<TPrimitiveId> : IEntityId where TPrimitiveId : IComparable
{
    public TPrimitiveId Value { get; }

    public EntityId(TPrimitiveId value)
    {
        Value = value;
    }

    public int CompareTo(EntityId<TPrimitiveId>? other) => Value.CompareTo(other!.Value);
    public override string ToString() => Value.ToString() ?? string.Empty;
    public static implicit operator string(EntityId<TPrimitiveId> id) => id?.Value.ToString() ?? string.Empty;
}