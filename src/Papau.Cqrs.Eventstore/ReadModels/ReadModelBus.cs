using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace Papau.Cqrs.Domain.ReadModels;

public class ReadModelBus : IReadModelBus
{
    public IEnumerable<IReadModelSubscriber> Subscribers { get; }
    public ILogger<ReadModelBus> Logger { get; }

    public ReadModelBus(IEnumerable<IReadModelSubscriber> subscribers, ILogger<ReadModelBus> logger)
    {
        Subscribers = subscribers ?? throw new ArgumentNullException(nameof(subscribers));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Publish(IEvent e)
    {
        foreach (var subscriber in Subscribers)
        {
            try
            {
                await subscriber.Handle(e).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Subscriber {subscriberType} couldn't handle event {@event}", subscriber.GetType().Name, e);
            }
        }
    }
}