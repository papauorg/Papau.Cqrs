using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Papau.Cqrs.Domain.Aggregates
{
    public class AggregateRepository<TAggregate> where TAggregate : AggregateRoot
    {
        public IAggregateFactory AggregateFactory { get; }
        public IEventPublisher PublishEndpoint { get; }

        public AggregateRepository(IAggregateFactory factory, IEventPublisher publishEndpoint)
        {
            AggregateFactory = factory ?? throw new System.ArgumentNullException(nameof(factory));
            PublishEndpoint = publishEndpoint ?? throw new System.ArgumentNullException(nameof(publishEndpoint));
        }

        protected Task<TAggregate> BuildFromHistory(string aggregateId, IEnumerable<IEvent> history)
        {
            if (history == null || !history.Any())
                throw new AggregateNotFoundException(aggregateId, typeof(TAggregate));

            var result = AggregateFactory.CreateAggregate<TAggregate>();
            result.ApplyChanges(history);

            return Task.FromResult(result);
        }

        protected async Task<IEnumerable<IEvent>> CommitAndPublish(string aggregateId, IEnumerable<IEvent> existingEvents, TAggregate aggregate)
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

        protected Task Save(string aggregateId, TAggregate aggregateToSave)
        {
            return null;
            // var changesToPersist = aggregateToSave.GetUncommittedChanges();
            // try
            // {
            //     var originalAggregate = GetById(aggregateId);
            //     var versionOfAggregateToSave = originalAggregate.Version - changesToPersist.Count -1; 

            //     if (originalVersion <>)
            // }
            // catch (AggregateNotFoundException ex)
            // {

            // }

            // if (_usersStore.TryGetValue(aggregateId, out var events))
            // {

            //     if (events.Count > user.Version - changesToPersist.Count() -1)
            //     {
            //         throw new AggregateVersionException();
            //     }
            //     events.AddRange(changesToPersist);
            //     _usersStore[aggregateId] = events;
            // }
            // else 
            // {
            //     _usersStore[aggregateId] = changesToPersist.ToList();
            // }

            // user.ClearUncommittedChanges();

            // await PublishEndpoint.Publish(events);
        }


    }
}