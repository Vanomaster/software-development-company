using System;
using Dal;
using Microsoft.EntityFrameworkCore;

namespace Queries.Base;

/// <summary>
/// Base query.
/// </summary>
/// <typeparam name="TModel">Query model.</typeparam>
/// <typeparam name="TResult">Query result.</typeparam>
public abstract class QueryBase<TModel, TResult> : IQuery<TModel, TResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QueryBase{TModel, TResult}"/> class.
    /// </summary>
    /// <param name="contextFactory">Context factory.</param>
    protected QueryBase(IDbContextFactory<Context> contextFactory)
    {
        ContextFactory = contextFactory;
    }

    /// <summary>
    /// Context.
    /// </summary>
    protected Context Context { get; private set; } = null!;

    private IDbContextFactory<Context> ContextFactory { get; }

    /// <inheritdoc/>
    public QueryResult<TResult> Execute(TModel model)
    {
        try
        {
            Context = ContextFactory.CreateDbContext();
            return ExecuteCore(model);
        }
        catch (Exception exception)
        {
            return GetFailedResult(exception.Message);
        }
        finally
        {
            Context?.Dispose();
        }
    }

    /// <summary>
    /// Execute query core.
    /// </summary>
    /// <param name="model">Query model.</param>
    /// <returns>Query result.</returns>
    protected abstract QueryResult<TResult> ExecuteCore(TModel model);

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