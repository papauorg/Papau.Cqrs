using System;

using EventStore.Client;

using Microsoft.Extensions.DependencyInjection;

using Papau.Cqrs.Domain.Aggregates;
using Papau.Cqrs.ReadModels;

namespace Papau.Cqrs.EventStore;

public static class StartupExtensions
{
    public class EventStoreConfigurationBuilder
    {
        private readonly IServiceCollection _services;

        public EventStoreConfigurationBuilder(IServiceCollection services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
        }

        public EventStoreConfigurationBuilder ForRepositories()
        {
            _services.AddScoped(typeof(IAggregateRepository<>), typeof(EventStoreRepository<>));
            _services.AddTransient<IEventSerializer, EventSerializer>();
            return this;
        }

        public EventStoreConfigurationBuilder ForReadModelStream()
        {
            _services.AddSingleton<EventStoreReadmodelStreamer>();
            _services.AddHostedService<EventStoreStreamerBackgroundService>();
            _services.AddSingleton<IReadModelBus, ReadModelBus>();
            return this;
        }
    }
    
    public static IServiceCollection AddEventstore(this IServiceCollection services, string connectionString, Action<EventStoreConfigurationBuilder> configure)
    {
        var clientSettings = EventStoreClientSettings
            .Create(connectionString);
        var client = new EventStoreClient(clientSettings);
        services.AddSingleton(client);

        configure?.Invoke(new EventStoreConfigurationBuilder(services));

        return services;
    }
}