using Papau.Cqrs.Domain;

namespace Papau.Cqrs.Tests.Data.LiteDbRepository;

public interface ILiteDbSampleEvent : IEvent
{
    TestId Id { get; }
    string SomeProp { get; }
}