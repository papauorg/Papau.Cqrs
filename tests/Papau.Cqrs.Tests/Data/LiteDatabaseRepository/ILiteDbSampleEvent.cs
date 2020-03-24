using System;
using Papau.Cqrs.Domain;

namespace Papau.Cqrs.Tests.Data.LiteDbRepository
{
    public interface ILiteDbSampleEvent : IEvent
    {
        Guid Id { get; }
        string SomeProp { get; }
    }
}