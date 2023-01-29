using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dal;
using Dal.Entities;
using Microsoft.EntityFrameworkCore;
using Queries.Base;

namespace Queries;

/// <inheritdoc />
public class OrdersQueryByCustomerLogin : AsyncQueryBase<string, List<Order>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OrdersQuery"/> class.
    /// </summary>
    /// <param name="contextFactory">Context factory.</param>
    public OrdersQueryByCustomerLogin(IDbContextFactory<Context> contextFactory)
        : base(contextFactory)
    {
    }


    /// <inheritdoc/>
    protected override async Task<QueryResult<List<Order>>> ExecuteCoreAsync(string? login)
    {
        var entitiesToFetch = Context.Orders.AsNoTracking();
        if (login is not null)
        {
            entitiesToFetch = entitiesToFetch.Where(entity => entity.Customer.User.Login == login);
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