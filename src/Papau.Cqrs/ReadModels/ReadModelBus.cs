using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Papau.Cqrs.Domain;

namespace Papau.Cqrs.ReadModels;

public class ReadModelBus : IReadModelBus
{
    public IEnumerable<IReadModelSubscriber> Subscribers { get; }
    public ILogger<ReadModelBus> Logger { get; }

    public ReadModelBus(IEnumerable<IReadModelSubscriber> subscribers, ILogger<ReadModelBus> logger)
    {
        Subscribers = subscribers ?? throw new ArgumentNullException(nameof(subscribers));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Publish(IEvent e, CancellationToken cancellationToken)
    {
        foreach (var subscriber in Subscribers)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                await subscriber.Handle(e, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Subscriber {subscriberType} couldn't handle event {@event}", subscriber.GetType().Name, e);
            }
        }
    }
}