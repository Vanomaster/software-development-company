namespace Queries.Base;

/// <summary>
/// Interface of query.
/// </summary>
/// <typeparam name="TModel">Type of query model.</typeparam>
/// <typeparam name="TResult">Type of query result.</typeparam>
public interface IQuery<in TModel, TResult>
{
    /// <summary>
    /// Execute query.
    /// </summary>
    /// <param name="model">Query model.</param>
    /// <returns>Query result.</returns>
    QueryResult<TResult> Execute(TModel model);
}