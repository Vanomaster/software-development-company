using System.Collections.Generic;
using System.Threading.Tasks;
using Commands.Base;
using Dal;
using Dal.Entities;
using Microsoft.EntityFrameworkCore;

namespace Commands;

public class AddOrUpdateEntityCommand : AsyncCommandBase<IEnumerable<IEntity>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddOrUpdateEntityCommand"/> class.
    /// </summary>
    /// <param name="contextFactory">Context factory.</param>
    public AddOrUpdateEntityCommand(IDbContextFactory<Context> contextFactory)
        : base(contextFactory)
    {
    }

    /// <inheritdoc/>
    protected override async Task<CommandResult> ExecuteCoreAsync(IEnumerable<IEntity> entities)
    {
        Context.UpdateRange(entities);
        await Context.SaveChangesAsync();

        return GetSuccessfulResult();
    }
}