using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dal;
using Dal.Entities;
using Microsoft.EntityFrameworkCore;
using Queries.Base;

namespace Queries;

/// <inheritdoc />
public class UsersQuery : AsyncQueryBase<List<Guid>?, List<User>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OrdersQuery"/> class.
    /// </summary>
    /// <param name="contextFactory">Context factory.</param>
    public UsersQuery(IDbContextFactory<Context> contextFactory)
        : base(contextFactory)
    {
    }

    /// <inheritdoc/>
    protected override async Task<QueryResult<List<User>>> ExecuteCoreAsync(List<Guid>? userIds)
    {
        var entitiesToFetch = Context.Users.AsNoTracking();
        if (userIds is not null)
        {
            entitiesToFetch = entitiesToFetch.Where(entity => userIds.Contains(entity.Id));
        }

        var users = await entitiesToFetch
            .Include(entity => entity.Customer)
            .Include(entity => entity.Employee)
                .ThenInclude(entity => entity.Passport)
            .ToListAsync();

        return GetSuccessfulResult(users);
    }
}