// MIT License

// Copyright (c) 2021 EventStore Ltd.

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

// See: https://github.dev/EventStore/samples/blob/2829b0a90a6488e1eee73fad0be33a3ded7d13d2/CQRS_Flow/.NET/Core/Core/Events/EventBus.cs#L23-L51

using System;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Papau.Cqrs.Domain;
using System.Reflection;
using System.Collections.Concurrent;
using System.Linq;

namespace Papau.Cqrs.ReadModels;

public class ReadModelBus : IReadModelBus
{
    private static ConcurrentDictionary<Type, MethodInfo> PublishMethods { get; } = new ConcurrentDictionary<Type, MethodInfo>();

    public IServiceProvider ServiceProvider { get; }
    public ILogger<ReadModelBus> Logger { get; }

    public ReadModelBus(IServiceProvider serviceProvider, ILogger<ReadModelBus> logger)
    {
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private async Task Publish<TEvent>(TEvent e, CancellationToken cancellationToken) where TEvent : IEvent
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        using var scope = ServiceProvider.CreateAsyncScope();
        var subscribers = scope.ServiceProvider.GetServices<IReadModelSubscriber<TEvent>>();

        foreach(var subscriber in subscribers)
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

    public Task Publish(IEvent @event, CancellationToken ct)
    {
        return (Task)GetGenericPublishFor(@event)
            .Invoke(this, new object[] { @event, ct })!;
    }

    private static MethodInfo GetGenericPublishFor(IEvent @event)
    {
        return PublishMethods.GetOrAdd(@event.GetType(), eventType =>
            typeof(ReadModelBus)
                .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .Single(m => m.Name == nameof(Publish) && m.GetGenericArguments().Any())
                .MakeGenericMethod(eventType)
        );
    }
}