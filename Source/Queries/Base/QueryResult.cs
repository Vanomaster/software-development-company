namespace Queries.Base;

/// <summary>
/// Result of query.
/// </summary>
/// <typeparam name="TResult">Type of result data.</typeparam>
public class QueryResult<TResult> : IQueryResult<TResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QueryResult{T}"/> class.
    /// </summary>
    /// <param name="data">.</param>
    public QueryResult(TResult data)
    {
        Data = data;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryResult{T}"/> class.
    /// </summary>
    /// <param name="errorMessage">Message of error.</param>
    public QueryResult(string errorMessage)
    {
        ErrorMessage = errorMessage;
    }

    /// <inheritdoc/>
    public TResult Data { get; } = default!;

    /// <inheritdoc/>
    public string ErrorMessage { get; } = null!;

    /// <inheritdoc/>
    public bool IsSuccessful => ErrorMessage == null;
}