using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Papau.Cqrs.Domain.Aggregates
{
    public abstract class AggregateRepository<TAggregate> 
        : IAggregateRepository, IAggregateRepository<TAggregate> where TAggregate : AggregateRoot
    {
        public IAggregateFactory AggregateFactory { get; }
        public IEventPublisher PublishEndpoint { get; }

        public AggregateRepository(IAggregateFactory factory, IEventPublisher publishEndpoint)
        {
            AggregateFactory = factory ?? throw new System.ArgumentNullException(nameof(factory));
            PublishEndpoint = publishEndpoint ?? throw new System.ArgumentNullException(nameof(publishEndpoint));
        }

        protected Task<AggregateRoot> BuildFromHistory(Type aggregateType, string aggregateId, IEnumerable<IEvent> history)
        {
            if (history == null || !history.Any())
                throw new AggregateNotFoundException(aggregateId, typeof(TAggregate));

            var result = AggregateFactory.CreateAggregate(aggregateType);
            
            result.ApplyChanges(history);

            return Task.FromResult(result);
        }

        protected async Task<IEnumerable<IEvent>> CommitAndPublish(string aggregateId, IEnumerable<IEvent> existingEvents, AggregateRoot aggregate)
        {
            var uncommittedEvents = aggregate.GetUncommittedChanges();
            var versionBeforeChanges = aggregate.Version - uncommittedEvents.Count();

            var currentlySavedVersion = existingEvents?.Count() ?? 0;

            if (versionBeforeChanges != currentlySavedVersion)
                throw new AggregateVersionException(aggregateId, typeof(TAggregate), versionBeforeChanges, currentlySavedVersion);

            aggregate.ClearUncommittedChanges();
            await PublishEndpoint.Publish(uncommittedEvents);

            if (versionBeforeChanges == 0)
            {
                return uncommittedEvents;
            }
            else
            {
                return existingEvents.Concat(uncommittedEvents);
            }
        }

        public async Task Save(TAggregate aggregateRoot)
        {
            await SaveInternal(aggregateRoot);
        }

        public async Task Save(AggregateRoot aggregateRoot)
        {
            await SaveInternal(aggregateRoot);
        }

        protected abstract Task SaveInternal(AggregateRoot aggregateRoot);

        public abstract Task<IEnumerable<IEvent>> GetAllEvents();
        
        public abstract Task<AggregateRoot> GetById(Type aggregateType, string aggregateId);

        public async Task<TAggregate> GetById(string aggregateId)
        {
            return (TAggregate)(await GetById(typeof(TAggregate), aggregateId));
        }
    }
}