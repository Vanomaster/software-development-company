using System.Threading.Tasks;

namespace Commands.Base;

/// <summary>
/// Async command.
/// </summary>
/// <typeparam name="TArgs">Type of command arguments.</typeparam>
/// <typeparam name="TResult">Type of command result.</typeparam>
public interface IAsyncCommand<in TArgs, TResult>
{
    /// <summary>
    /// Execute command async.
    /// </summary>
    /// <param name="args">Command arguments.</param>
    /// <returns>Command result.</returns>
    Task<CommandResult<TResult>> ExecuteAsync(TArgs args); // TODO CancellationToken
}