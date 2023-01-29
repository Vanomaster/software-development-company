using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dal;
using Dal.Entities;
using Microsoft.EntityFrameworkCore;
using Queries.Base;

namespace Queries;

/// <inheritdoc />
public class CustomersQuery : AsyncQueryBase<List<Guid>, List<Customer>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OrdersQuery"/> class.
    /// </summary>
    /// <param name="contextFactory">Context factory.</param>
    public CustomersQuery(IDbContextFactory<Context> contextFactory)
        : base(contextFactory)
    {
    }

    /// <inheritdoc/>
    protected override async Task<QueryResult<List<Customer>>> ExecuteCoreAsync(List<Guid>? userIds)
    {
        var entitiesToFetch = Context.Customers.AsNoTracking();
        if (userIds is not null)
        {
            entitiesToFetch = entitiesToFetch.Where(entity => userIds.Contains(entity.Id));
        }

        var customers = await entitiesToFetch.Include(entity => entity.User).ToListAsync();

        return GetSuccessfulResult(customers);
    }
}