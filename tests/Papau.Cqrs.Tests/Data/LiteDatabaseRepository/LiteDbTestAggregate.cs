using System;

using Papau.Cqrs.Domain.Aggregates;

namespace Papau.Cqrs.Tests.Data.LiteDbRepository;

public class LiteDbTestAggregate : AggregateRoot<TestId>
{
    public string SampleProperty { get; private set; }

    public LiteDbTestAggregate() : base(new TestId(Guid.NewGuid()))
    {
        Handle<ILiteDbSampleEvent>(SampleEventHandler);
    }

    public void AddSampleEvent()
    {
        ApplyChange(new LiteDbSampleEvent
        {
            Id = Id,
            SomeProp = "someProp"
        });
    }

    private void SampleEventHandler(ILiteDbSampleEvent sampleEvent)
    {
        Id = sampleEvent.Id;
        SampleProperty = sampleEvent.SomeProp;
    }
}