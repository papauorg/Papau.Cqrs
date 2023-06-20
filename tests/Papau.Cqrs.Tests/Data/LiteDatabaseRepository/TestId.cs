using System;

using Papau.Cqrs.Domain.Entities;

namespace Papau.Cqrs.Tests.Data.LiteDbRepository;

public record TestId : EntityId<Guid>
{
    public TestId(Guid value) : base(value)
    {
    }
}