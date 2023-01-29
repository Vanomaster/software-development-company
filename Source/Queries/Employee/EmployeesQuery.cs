using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dal;
using Dal.Entities;
using Microsoft.EntityFrameworkCore;
using Queries.Base;

namespace Queries;

/// <inheritdoc />
public class EmployeesQuery : AsyncQueryBase<List<Guid>, List<Employee>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OrdersQuery"/> class.
    /// </summary>
    /// <param name="contextFactory">Context factory.</param>
    public EmployeesQuery(IDbContextFactory<Context> contextFactory)
        : base(contextFactory)
    {
    }

    /// <inheritdoc/>
    protected override async Task<QueryResult<List<Employee>>> ExecuteCoreAsync(List<Guid>? employeesIds)
    {
        var entitiesToFetch = Context.Employees.AsNoTracking();
        if (employeesIds is not null)
        {
            entitiesToFetch = entitiesToFetch.Where(entity => employeesIds.Contains(entity.Id));
        }

        var employees = await entitiesToFetch
            .Include(entity => entity.User)
            .Include(entity => entity.Passport)
            .ToListAsync();

        return GetSuccessfulResult(employees);
    }
}