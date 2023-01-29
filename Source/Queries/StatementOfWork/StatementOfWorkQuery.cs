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
public class StatementOfWorksQuery : AsyncQueryBase<List<Guid>, List<StatementOfWork>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OrdersQuery"/> class.
    /// </summary>
    /// <param name="contextFactory">Context factory.</param>
    public StatementOfWorksQuery(IDbContextFactory<Context> contextFactory)
        : base(contextFactory)
    {
    }

    /// <inheritdoc/>
    protected override async Task<QueryResult<List<StatementOfWork>>> ExecuteCoreAsync(List<Guid>? statementOfWorkIds)
    {
        var entitiesToFetch = Context.StatementOfWorks.AsNoTracking();
        if (statementOfWorkIds is not null)
        {
            entitiesToFetch = entitiesToFetch.Where(entity => statementOfWorkIds.Contains(entity.Id));
        }

        var statementOfWorks = await entitiesToFetch.ToListAsync();

        return GetSuccessfulResult(statementOfWorks);
    }
}