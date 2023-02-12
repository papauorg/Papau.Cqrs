using System.Threading.Tasks;

namespace Papau.Cqrs.Domain.ReadModels;

public interface IReadModelSubscriber
{
    Task Handle(IEvent e);
}