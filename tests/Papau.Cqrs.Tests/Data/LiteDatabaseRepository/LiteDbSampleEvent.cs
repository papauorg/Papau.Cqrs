using System;
using Papau.Cqrs.Domain;

namespace Papau.Cqrs.Tests.Data.LiteDbRepository
{
    public class LiteDbSampleEvent : ILiteDbSampleEvent
    {
        public Guid Id { get; set; }
        public string SomeProp { get; set; }
    }
}