using System.Threading.Tasks;

namespace Queries.Base;

public interface IAsyncQuery<TResult>
{
    Task<QueryResult<TResult>> ExecuteAsync(); // TODO CancellationToken
}