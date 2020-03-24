using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LiteDB;
using Papau.Cqrs.Domain;
using Papau.Cqrs.Domain.Aggregates;

namespace Papau.Cqrs.LiteDb.Domain.Aggregates
{
    public class LiteDbAggregateRepository<TAggregate>
        : AggregateRepository<TAggregate> where TAggregate : AggregateRoot
    {
        public ILiteDatabase LiteDb { get; }
        
        public LiteDbAggregateRepository(
            IAggregateFactory factory, 
            IEventPublisher publishEndpoint,
            ILiteDatabase liteDb) : base(factory, publishEndpoint)
        {
            LiteDb = liteDb ?? throw new System.ArgumentNullException(nameof(liteDb));
        }

        public override Task<IEnumerable<IEvent>> GetAllEvents()
        {
            var eventCollection = LiteDb.GetCollection("AllEvents");
            var allEvents = eventCollection.FindAll();
            
            return Task.FromResult(Deserialize(allEvents));
        }

        public override Task<AggregateRoot> GetById(Type aggregateType, string aggregateId)
        {
            var result = new List<IEvent>();

            var eventCollection = LiteDb.GetCollection("AllEvents");
            var aggregateEvents = eventCollection.Find(Query.EQ("_AggregateId", aggregateId));
            
            var typedEvents = Deserialize(aggregateEvents);

            return BuildFromHistory(aggregateType, aggregateId, typedEvents);
        }

        protected override Task SaveInternal(AggregateRoot aggregateRoot)
        {
            var aggregateId = aggregateRoot.GetId();
            var uncommittedEvents = aggregateRoot.GetUncommittedChanges();
            var eventsToSave = uncommittedEvents
                .Select(e => {
                    var doc = LiteDb.Mapper.ToDocument(e.GetType(), e);
                    
                    if (doc.ContainsKey("_id"))
                    {
                        doc["_EventId"] = doc["_id"];
                        doc.Remove("_id"); // always use autoid to keep order
                    }
                    doc["_EventType"] = e.GetType().AssemblyQualifiedName;
                    doc["_AggregateId"] = aggregateId;
                    return doc;
                })
                .ToList();

            if (!eventsToSave.Any())
                return Task.CompletedTask;

            var userEventCollection = LiteDb.GetCollection("AllEvents");
            userEventCollection.EnsureIndex("_AggregateId");
            
            userEventCollection.InsertBulk(eventsToSave);
            PublishEndpoint.Publish(uncommittedEvents);
            
            aggregateRoot.ClearUncommittedChanges();

            return Task.CompletedTask;
        }

        private IEnumerable<IEvent> Deserialize(IEnumerable<BsonDocument> documents)
        {
            return documents.Select(e => {
                var type = ResolveType(e["_EventType"].AsString);
                if (e.ContainsKey("_EventId"))
                    e["_id"] = e["_EventId"]; // fix id serialization hack
                return (IEvent)LiteDb.Mapper.ToObject(type, e);
            });
        }

        protected virtual Type ResolveType(string typeName)
        {
            return Type.GetType(typeName, throwOnError: true, ignoreCase: true);
        }
    }
}