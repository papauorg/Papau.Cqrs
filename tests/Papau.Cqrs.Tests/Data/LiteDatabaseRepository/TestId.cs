using System;

using Papau.Cqrs.Domain.Aggregates;

namespace Papau.Cqrs.Tests.Data.LiteDbRepository;

public class TestId : AggregateId<Guid>
{
    public TestId(Guid value) : base(value)
    {
    }
}