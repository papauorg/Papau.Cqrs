using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Papau.Cqrs.EventStore;

internal class EventStoreStreamerBackgroundService : IHostedService
{
    private CancellationTokenSource _tokenSource;

    public EventStoreReadmodelStreamer Streamer { get; }

    public EventStoreStreamerBackgroundService(EventStoreReadmodelStreamer streamer)
    {
        Streamer = streamer ?? throw new System.ArgumentNullException(nameof(streamer));
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _tokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        await Streamer.SubscribeToAll(_tokenSource.Token).ConfigureAwait(false);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        // cancel subscription
        _tokenSource.Cancel();

        return Task.CompletedTask;
    }
}
