using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dal;
using Dal.Entities;
using Microsoft.EntityFrameworkCore;
using Queries.Base;

namespace Queries;

/// <inheritdoc />
public class EmployeesQueryByOrderNumber : AsyncQueryBase<List<int>, List<Employee>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OrdersQuery"/> class.
    /// </summary>
    /// <param name="contextFactory">Context factory.</param>
    public EmployeesQueryByOrderNumber(IDbContextFactory<Context> contextFactory)
        : base(contextFactory)
    {
    }

    /// <inheritdoc/>
    protected override async Task<QueryResult<List<Employee>>> ExecuteCoreAsync(List<int> orderNumbers)
    {
        var entitiesToFetch = Context.Orders
            .Where(order => orderNumbers.Contains(order.Number))
            .SelectMany(order => order.Employees)
            .AsNoTracking();

        var employees = await entitiesToFetch
            .Include(entity => entity.User)
            .Include(entity => entity.Passport)
            .ToListAsync();

        return GetSuccessfulResult(employees);
    }
}