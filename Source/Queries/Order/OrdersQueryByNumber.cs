using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dal;
using Dal.Entities;
using Microsoft.EntityFrameworkCore;
using Queries.Base;

namespace Queries;

/// <inheritdoc />
public class OrdersQueryByNumber : AsyncQueryBase<List<int>, List<Order>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OrdersQuery"/> class.
    /// </summary>
    /// <param name="contextFactory">Context factory.</param>
    public OrdersQueryByNumber(IDbContextFactory<Context> contextFactory)
        : base(contextFactory)
    {
    }

    /// <inheritdoc/>
    protected override async Task<QueryResult<List<Order>>> ExecuteCoreAsync(List<int>? orderNumbers)
    {
        var entitiesToFetch = Context.Orders.AsNoTracking();
        if (orderNumbers is not null)
        {
            entitiesToFetch = entitiesToFetch.Where(entity => orderNumbers.Contains(entity.Number));
        }

        var orders = await entitiesToFetch
            .Include(entity => entity.Customer)
                .ThenInclude(customer => customer.User)
            .ToListAsync();

        return GetSuccessfulResult(orders);
    }
}