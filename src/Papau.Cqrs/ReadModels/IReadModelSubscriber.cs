using System.Threading;
using System.Threading.Tasks;

using Papau.Cqrs.Domain;

namespace Papau.Cqrs.ReadModels;

public interface IReadModelSubscriber
{
    Task Handle(IEvent e, CancellationToken cancellationToken);
}