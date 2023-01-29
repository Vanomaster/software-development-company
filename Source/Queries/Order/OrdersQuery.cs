using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dal;
using Dal.Entities;
using Microsoft.EntityFrameworkCore;
using Queries.Base;

namespace Queries;

/// <inheritdoc />
public class OrdersQuery : AsyncQueryBase<List<Guid>, List<Order>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OrdersQuery"/> class.
    /// </summary>
    /// <param name="contextFactory">Context factory.</param>
    public OrdersQuery(IDbContextFactory<Context> contextFactory)
        : base(contextFactory)
    {
    }

    /// <inheritdoc/>
    protected override async Task<QueryResult<List<Order>>> ExecuteCoreAsync(List<Guid>? orderIds)
    {
        var entitiesToFetch = Context.Orders.AsNoTracking();
        if (orderIds is not null)
        {
            entitiesToFetch = entitiesToFetch.Where(entity => orderIds.Contains(entity.Id));
        }

        var orders = await entitiesToFetch
            .Include(entity => entity.Customer)
                .ThenInclude(customer => customer.User)
            .Include(entity => entity.Employees)
            .Include(entity => entity.StatementOfWork)
            .ToListAsync();

        return GetSuccessfulResult(orders);
    }
}