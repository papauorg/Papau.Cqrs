using System;
using System.Threading;
using System.Threading.Tasks;

using EventStore.Client;

using Microsoft.Extensions.Logging;

using Papau.Cqrs.Domain.ReadModels;

namespace Papau.Cqrs.EventStore;

// inspired by: https://github.dev/EventStore/samples/tree/main/CQRS_Flow/.NET 
public sealed class EventStoreReadmodelStreamer
{
    private static class NoSynchronizationContextScope
    {
        public static Disposable Enter()
        {
            var context = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(null);
            return new Disposable(context);
        }

        public readonly struct Disposable : IDisposable
        {
            private readonly SynchronizationContext? synchronizationContext;

            public Disposable(SynchronizationContext? synchronizationContext)
            {
                this.synchronizationContext = synchronizationContext;
            }

            public void Dispose() =>
                SynchronizationContext.SetSynchronizationContext(synchronizationContext);
        }
    }

    public EventStoreClient EventStoreClient { get; }
    public IEventSerializer EventSerializer { get; }
    public IReadModelBus Bus { get; }
    public ILogger<EventStoreReadmodelStreamer> Logger { get; }
    public CancellationToken CancellationToken { get; private set; }

    private readonly object resubscribeLock = new();

    public EventStoreReadmodelStreamer(
        EventStoreClient eventStoreClient,
        IEventSerializer eventSerializer,
        IReadModelBus bus,
        ILogger<EventStoreReadmodelStreamer> logger)
    {
        EventStoreClient = eventStoreClient ?? throw new ArgumentNullException(nameof(eventStoreClient));
        EventSerializer = eventSerializer ?? throw new ArgumentNullException(nameof(eventSerializer));
        Bus = bus ?? throw new ArgumentNullException(nameof(bus));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SubscribeToAll(CancellationToken cancellationToken)
    {
        CancellationToken = cancellationToken;
        Logger.LogInformation("Subscription to all events for building readmodel.");

        ulong? checkpoint = null; //await checkpointRepository.Load(SubscriptionId, cancellationToken);
        var filterOptions = new SubscriptionFilterOptions(EventTypeFilter.ExcludeSystemEvents());
        if (checkpoint != null)
        {
            await EventStoreClient.SubscribeToAllAsync(
                FromAll.After(new Position(checkpoint.Value, checkpoint.Value)),
                HandleEvent,
                resolveLinkTos: false,
                subscriptionDropped: HandleDrop,
                filterOptions: filterOptions,
                cancellationToken: cancellationToken
            ).ConfigureAwait(false);
        }
        else
        {
            await EventStoreClient.SubscribeToAllAsync(
                FromAll.Start,
                HandleEvent,
                resolveLinkTos: false,
                filterOptions: filterOptions,
                subscriptionDropped: HandleDrop,
                cancellationToken: cancellationToken
            ).ConfigureAwait(false);
        }

        Logger.LogInformation("Subscription to all started");
    }

    private async Task HandleEvent(StreamSubscription subscription, ResolvedEvent resolvedEvent, CancellationToken cancellationToken)
    {
        try
        {
            if (IsEventWithEmptyData(resolvedEvent))
                return;

            await PublishEvent(resolvedEvent, cancellationToken).ConfigureAwait(false);

            //await checkpointRepository.Store(SubscriptionId, resolvedEvent.Event.Position.CommitPosition, cancellationToken);
        }
        catch (Exception e)
        {
            Logger.LogError("Error consuming message: {ExceptionMessage}{ExceptionStackTrace}", e.Message,
                e.StackTrace);
            // if you're fine with dropping some events instead of stopping subscription
            // then you can add some logic if error should be ignored
            throw;
        }
    }

    private void HandleDrop(StreamSubscription _, SubscriptionDroppedReason reason, Exception exception)
    {
        Logger.LogError(
            exception,
            "Subscription to all dropped with '{Reason}'",
            reason
        );

        Resubscribe();
    }

    private void Resubscribe()
    {
        // You may consider adding a max resubscribe count if you want to fail process
        // instead of retrying until database is up
        while (true)
        {
            var resubscribed = false;
            try
            {
                Monitor.Enter(resubscribeLock);

                // No synchronization context is needed to disable synchronization context.
                // That enables running asynchronous method not causing deadlocks.
                // As this is a background process then we don't need to have async context here.
                using (NoSynchronizationContextScope.Enter())
                {
                    SubscribeToAll(CancellationToken).Wait(CancellationToken);
                }

                resubscribed = true;
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Failed to resubscribe to all.");
            }
            finally
            {
                Monitor.Exit(resubscribeLock);
            }

            if (resubscribed)
                break;

            // Sleep between reconnections to not flood the database or not kill the CPU with infinite loop
            // Randomness added to reduce the chance of multiple subscriptions trying to reconnect at the same time
            Thread.Sleep(1000 + new Random((int)DateTime.UtcNow.Ticks).Next(1000));
        }
    }

    private static bool IsEventWithEmptyData(ResolvedEvent resolvedEvent)
        => resolvedEvent.Event.Data.Length == 0;

    private async Task PublishEvent(ResolvedEvent @event, CancellationToken cancellationToken)
    {

        if (!@event.Event.ContentType.Equals("application/json", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"The event '{@event.OriginalEventNumber}' in stream '{@event.OriginalStreamId}' can't be dispatched in the read model because it is not a json event.");

        var eventToDispatch = EventSerializer.DeserializeEvent(@event.Event);
        if (eventToDispatch == null)
            throw new InvalidOperationException($"The event '{@event.OriginalEventNumber}' in stream '{@event.OriginalStreamId}' can't be processed in the read model because it couldn't be deserialized.");

        await Bus.Publish(eventToDispatch, cancellationToken).ConfigureAwait(false);
    }
}