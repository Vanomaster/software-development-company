using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dal;
using Dal.Entities;
using Microsoft.EntityFrameworkCore;
using Queries.Base;

namespace Queries;

public class UsersQueryByLogin : AsyncQueryBase<List<string>?, List<User>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OrdersQuery"/> class.
    /// </summary>
    /// <param name="contextFactory">Context factory.</param>
    public UsersQueryByLogin(IDbContextFactory<Context> contextFactory)
        : base(contextFactory)
    {
    }

    /// <inheritdoc/>
    protected override async Task<QueryResult<List<User>>> ExecuteCoreAsync(List<string>? userLogins)
    {
        var entitiesToFetch = Context.Users.AsNoTracking();
        if (userLogins is not null)
        {
            entitiesToFetch = entitiesToFetch.Where(entity => userLogins.Contains(entity.Login));
        }

        var users = await entitiesToFetch.ToListAsync();

        return GetSuccessfulResult(users);
    }
}