using System.Threading.Tasks;

namespace Commands.Base;

/// <summary>
/// Async command.
/// </summary>
/// <typeparam name="TArgs">Type of command arguments.</typeparam>
public interface IAsyncCommand<in TArgs>
{
    /// <summary>
    /// Execute command async.
    /// </summary>
    /// <param name="args">Command arguments.</param>
    /// <returns>Command result.</returns>
    Task<CommandResult> ExecuteAsync(TArgs args); // TODO CancellationToken
}