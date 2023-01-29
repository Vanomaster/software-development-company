namespace Queries.Base;

/// <summary>
/// Interface of query.
/// </summary>
/// <typeparam name="TResult">Type of query result.</typeparam>
public interface IQuery<TResult>
{
    /// <summary>
    /// Execute query.
    /// </summary>
    /// <returns>Query result.</returns>
    QueryResult<TResult> Execute();
}