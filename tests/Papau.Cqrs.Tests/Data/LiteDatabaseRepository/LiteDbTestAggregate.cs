using System;

using Papau.Cqrs.Domain.Aggregates;
using Papau.Cqrs.Domain;

namespace Papau.Cqrs.Tests.Data.LiteDbRepository;

public class LiteDbTestAggregate : AggregateRoot<TestId>
{
    public string SampleProperty { get; private set; }

    public LiteDbTestAggregate() : base(new TestId(Guid.NewGuid()))
    {
    }

    protected override void Apply(IEvent @event) => Apply(@event as dynamic);

    public void AddSampleEvent()
    {
        RaiseEvent<LiteDbSampleEvent>(() => new() {
            Id = Id,
            SomeProp = "someProp"
        });
    }

    private void Apply(LiteDbSampleEvent sampleEvent)
    {
        Id = sampleEvent.Id;
        SampleProperty = sampleEvent.SomeProp;
    }
}