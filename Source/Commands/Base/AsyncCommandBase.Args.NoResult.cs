using System;
using System.Threading.Tasks;
using Dal;
using Microsoft.EntityFrameworkCore;

namespace Commands.Base;

public abstract class AsyncCommandBase<TArgs> : IAsyncCommand<TArgs>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncCommandBase{TArgs}"/> class.
    /// </summary>
    /// <param name="contextFactory">Context factory.</param>
    protected AsyncCommandBase(IDbContextFactory<Context> contextFactory)
    {
        ContextFactory = contextFactory;
    }

    /// <summary>
    /// Context.
    /// </summary>
    protected Context Context { get; private set; } = null!;

    private IDbContextFactory<Context> ContextFactory { get; }

    /// <inheritdoc/>
    public async Task<CommandResult> ExecuteAsync(TArgs args)
    {
        try
        {
            Context = await ContextFactory.CreateDbContextAsync();
            return await ExecuteCoreAsync(args);
        }
        catch (Exception exception)
        {
            return await Task.FromResult(GetFailedResult(exception.ToString())); // TODO message
        }
        // finally
        // {
        //     if (Context is not null)
        //     {
        //         await Context.DisposeAsync();
        //     }
        // }
    }

    /// <summary>
    /// Execute command core async.
    /// </summary>
    /// <param name="args">Command arguments.</param>
    /// <returns>Query result.</returns>
    protected abstract Task<CommandResult> ExecuteCoreAsync(TArgs args);

    /// <summary>
    /// Get successful result.
    /// </summary>
    /// <returns>Command result.</returns>
    protected CommandResult GetSuccessfulResult()
    {
        return new CommandResult();
    }

    /// <summary>
    /// Get failed result.
    /// </summary>
    /// <param name="errorMessage">Error message.</param>
    /// <returns>Command result.</returns>
    protected CommandResult GetFailedResult(string errorMessage)
    {
        return new CommandResult(errorMessage);
    }
}