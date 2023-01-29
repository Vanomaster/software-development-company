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
public class OrdersQueryByCustomer : AsyncQueryBase<List<Guid>, List<Order>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OrdersQuery"/> class.
    /// </summary>
    /// <param name="contextFactory">Context factory.</param>
    public OrdersQueryByCustomer(IDbContextFactory<Context> contextFactory)
        : base(contextFactory)
    {
    }

    /// <inheritdoc/>
    protected override async Task<QueryResult<List<Order>>> ExecuteCoreAsync(List<Guid>? customerIds)
    {
        var entitiesToFetch = Context.Orders.AsNoTracking();
        if (customerIds is not null)
        {
            entitiesToFetch = entitiesToFetch.Where(entity => customerIds.Contains(entity.Customer.Id));
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