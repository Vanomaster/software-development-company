using System;
using System.Threading.Tasks;
using Dal;
using Microsoft.EntityFrameworkCore;

namespace Queries.Base;

/// <summary>
/// Base async query.
/// </summary>
/// <typeparam name="TModel">Query model.</typeparam>
/// <typeparam name="TResult">Query result.</typeparam>
public abstract class AsyncQueryBase<TModel, TResult> : IAsyncQuery<TModel, TResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QueryBase{TModel, TResult}"/> class.
    /// </summary>
    /// <param name="contextFactory">Context factory.</param>
    protected AsyncQueryBase(IDbContextFactory<Context> contextFactory)
    {
        ContextFactory = contextFactory;
    }

    /// <summary>
    /// Context.
    /// </summary>
    protected Context Context { get; private set; } = null!;

    private IDbContextFactory<Context> ContextFactory { get; }

    /// <inheritdoc/>
    public async Task<QueryResult<TResult>> ExecuteAsync(TModel model = default!)
    {
        try
        {
            Context = await ContextFactory.CreateDbContextAsync();
            return await ExecuteCoreAsync(model);
        }
        catch (Exception exception)
        {
            return await Task.FromResult(GetFailedResult(exception.Message));
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
    /// Execute async query core.
    /// </summary>
    /// <param name="model">Query model.</param>
    /// <returns>Query result.</returns>
    protected abstract Task<QueryResult<TResult>> ExecuteCoreAsync(TModel model);

    /// <summary>
    /// Get successful result.
    /// </summary>
    /// <param name="data">Data.</param>
    /// <returns>Query result.</returns>
    protected QueryResult<TResult> GetSuccessfulResult(TResult data)
    {
        return new QueryResult<TResult>(data: data);
    }

    /// <summary>
    /// Get failed result.
    /// </summary>
    /// <param name="errorMessage">Error message.</param>
    /// <returns>Query result.</returns>
    protected QueryResult<TResult> GetFailedResult(string errorMessage)
    {
        return new QueryResult<TResult>(errorMessage);
    }
}