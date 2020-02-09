using System.Collections.Generic;
using System.Threading.Tasks;

namespace Papau.Cqrs.Domain
{
    public interface IEventPublisher
    {
        Task Publish(IEvent eventToPublish);
        Task Publish(IEnumerable<IEvent> eventsToPublish);
    }
}