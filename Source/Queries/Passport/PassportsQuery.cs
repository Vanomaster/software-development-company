using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dal;
using Dal.Entities;
using Microsoft.EntityFrameworkCore;
using Queries.Base;

namespace Queries;

/// <inheritdoc />
public class PassportsQuery : AsyncQueryBase<List<PassportsQueryModel>, List<Passport>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OrdersQuery"/> class.
    /// </summary>
    /// <param name="contextFactory">Context factory.</param>
    public PassportsQuery(IDbContextFactory<Context> contextFactory)
        : base(contextFactory)
    {
    }

    /// <inheritdoc/>
    protected override async Task<QueryResult<List<Passport>>> ExecuteCoreAsync(List<PassportsQueryModel>? passportIds)
    {
        var entitiesToFetch = Context.Passports.AsNoTracking();
        if (passportIds is not null)
        {
            entitiesToFetch = entitiesToFetch
                .Where(entity => passportIds.Select(model => model.Series).Contains(entity.Series)
                                 && passportIds.Select(model => model.Number).Contains(entity.Number));
        }

        var passports = await entitiesToFetch.ToListAsync();

        return GetSuccessfulResult(passports);
    }
}