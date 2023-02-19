using System.Threading;
using System.Threading.Tasks;

using Papau.Cqrs.Domain;

namespace Papau.Cqrs.ReadModels;

public interface IReadModelSubscriber<TEvent> where TEvent : IEvent
{
    Task Handle(TEvent e, CancellationToken cancellationToken);
}