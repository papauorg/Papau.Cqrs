using System.Threading.Tasks;

namespace Papau.Cqrs.Domain;

/// <summary>
/// Interface for simplifying sending commands
/// </summary>
public interface ICommandSender
{
    /// <summary>
    /// Send the given command to the correct location.
    /// </summary>
    /// <param name="command">Command to be sent</param>
    /// <returns></returns>
    Task Send<TCommand>(TCommand command) where TCommand : class, ICommand;

    /// <summary>
    /// Maps the objects properties to the command interface type and sends it to the bus
    /// </summary>
    /// <param name="command">object that can map to the command interface</param>
    /// <typeparam name="TCommand">command interface</typeparam>
    /// <returns></returns>
    Task Send<TCommand>(object command) where TCommand : class, ICommand;
}