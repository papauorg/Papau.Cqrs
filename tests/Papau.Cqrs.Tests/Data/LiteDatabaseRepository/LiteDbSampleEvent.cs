using Papau.Cqrs.Domain;

namespace Papau.Cqrs.Tests.Data.LiteDbRepository;

public record class LiteDbSampleEvent : IEvent
{
    public TestId Id { get; init; }
    public string SomeProp { get; init; }
}