namespace Papau.Cqrs.Tests.Data.LiteDbRepository;

public class LiteDbSampleEvent : ILiteDbSampleEvent
{
    public TestId Id { get; set; }
    public string SomeProp { get; set; }
}