using System.Threading;
using System.Threading.Tasks;

namespace Papau.Cqrs.Domain.ReadModels;

/// Interface for reading events from the store and (re-)building read models.
public interface IReadModelBus
{
    Task Publish(IEvent e, CancellationToken cancellationToken);
}