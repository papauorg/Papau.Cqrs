using System.Threading;
using System.Threading.Tasks;

using Papau.Cqrs.Domain;

namespace Papau.Cqrs.ReadModels;

/// Interface for reading events from the store and (re-)building read models.
public interface IReadModelBus
{
    Task Publish(IEvent e, CancellationToken cancellationToken);
}