using System.Collections.Generic;
using System.Threading.Tasks;
using Commands.Base;
using Dal;
using Dal.Entities;
using Microsoft.EntityFrameworkCore;

namespace Commands;

public class SaveChangesEntityCommand : AsyncCommandBase<IEnumerable<IEntity>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SaveChangesEntityCommand"/> class.
    /// </summary>
    /// <param name="contextFactory">Context factory.</param>
    public SaveChangesEntityCommand(IDbContextFactory<Context> contextFactory)
        : base(contextFactory)
    {
    }

    /// <inheritdoc/>
    protected override async Task<CommandResult> ExecuteCoreAsync(IEnumerable<IEntity> entities)
    {
        await Context.SaveChangesAsync();

        return GetSuccessfulResult();
    }
}