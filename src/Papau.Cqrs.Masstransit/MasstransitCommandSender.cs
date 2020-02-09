using System;
using System.Threading.Tasks;
using MassTransit;
using Papau.Cqrs.Domain;

namespace Papau.Cqrs.Masstransit
{
    public class MasstransitCommandSender : ICommandSender
    {
        public ISendEndpointProvider SendEndpointProvider { get; }
        public MasstransitCommandSender(ISendEndpointProvider sendEndpointProvider)
        {
            SendEndpointProvider = sendEndpointProvider ?? throw new System.ArgumentNullException(nameof(sendEndpointProvider));
        }

        public async Task Send<TCommand>(TCommand command) where TCommand : class, ICommand
        {
            await SendEndpointProvider.Send(command);
        }

        public async Task Send<TCommand>(object command) where TCommand : class, ICommand
        {
            await SendEndpointProvider.Send<TCommand>(command);
        }
    }
}