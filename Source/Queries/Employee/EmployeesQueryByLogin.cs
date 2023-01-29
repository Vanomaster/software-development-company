using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dal;
using Dal.Entities;
using Microsoft.EntityFrameworkCore;
using Queries.Base;

namespace Queries;

/// <inheritdoc />
public class EmployeesQueryByLogin : AsyncQueryBase<List<string>, List<Employee>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OrdersQuery"/> class.
    /// </summary>
    /// <param name="contextFactory">Context factory.</param>
    public EmployeesQueryByLogin(IDbContextFactory<Context> contextFactory)
        : base(contextFactory)
    {
    }

    /// <inheritdoc/>
    protected override async Task<QueryResult<List<Employee>>> ExecuteCoreAsync(List<string>? employeeLogins)
    {
        var entitiesToFetch = Context.Employees.AsNoTracking();
        if (employeeLogins is not null)
        {
            entitiesToFetch = entitiesToFetch.Where(entity => employeeLogins.Contains(entity.User.Login));
        }

        var employees = await entitiesToFetch
            .Include(entity => entity.User)
            .Include(entity => entity.Passport)
            .ToListAsync();

        return GetSuccessfulResult(employees);
    }
}