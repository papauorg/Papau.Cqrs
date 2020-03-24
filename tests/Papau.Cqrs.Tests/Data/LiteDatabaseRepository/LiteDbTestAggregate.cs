using System;
using Papau.Cqrs.Domain.Aggregates;

namespace Papau.Cqrs.Tests.Data.LiteDbRepository
{
    public class LiteDbTestAggregate : AggregateRoot
    {
        public Guid Id {get; private set;}
        public string SampleProperty { get; private set; }

        public LiteDbTestAggregate()
        {
            Handle<ILiteDbSampleEvent>(SampleEventHandler);
        }

        public void AddSampleEvent()
        {
            ApplyChange(new LiteDbSampleEvent{
                Id = Id,
                SomeProp = "someProp"
            });
        }

        private void SampleEventHandler(ILiteDbSampleEvent sampleEvent)
        {
            Id = sampleEvent.Id;
            SampleProperty = sampleEvent.SomeProp;
        }

        public override string GetId()
        {
            return $"LdbAgg-{Id}";
        }
    }
}