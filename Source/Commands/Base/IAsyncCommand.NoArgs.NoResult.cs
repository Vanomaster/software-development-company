using System.Threading.Tasks;

namespace Commands.Base;

/// <summary>
/// Async command.
/// </summary>
public interface IAsyncCommand
{
    /// <summary>
    /// Execute command async.
    /// </summary>
    /// <returns>Command result.</returns>
    Task<CommandResult> ExecuteAsync(); // TODO CancellationToken
}