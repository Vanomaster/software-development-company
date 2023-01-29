using System.Threading.Tasks;

namespace Queries.Base;

public interface IAsyncQuery<in TModel, TResult>
{
    Task<QueryResult<TResult>> ExecuteAsync(TModel model); // TODO CancellationToken
}