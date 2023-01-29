using System.Collections.Generic;
using System.Threading.Tasks;
using Commands.Base;
using Dal;
using Dal.Entities;
using Microsoft.EntityFrameworkCore;

namespace Commands;

public class AddEntityCommand : AsyncCommandBase<IEnumerable<IEntity>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddEntityCommand"/> class.
    /// </summary>
    /// <param name="contextFactory">Context factory.</param>
    public AddEntityCommand(IDbContextFactory<Context> contextFactory)
        : base(contextFactory)
    {
    }

    /// <inheritdoc/>
    protected override async Task<CommandResult> ExecuteCoreAsync(IEnumerable<IEntity> entities)
    {
        await Context.AddRangeAsync(entities);
        await Context.SaveChangesAsync();

        return GetSuccessfulResult();
    }
}