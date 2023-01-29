using System.Collections.Generic;
using System.Threading.Tasks;
using Commands.Base;
using Dal;
using Dal.Entities;
using Microsoft.EntityFrameworkCore;

namespace Commands;

public class DeleteNonStandardEntityCommand<T> : AsyncCommandBase<IEnumerable<T>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteNonStandardEntityCommand"/> class.
    /// </summary>
    /// <param name="contextFactory">Context factory.</param>
    public DeleteNonStandardEntityCommand(IDbContextFactory<Context> contextFactory)
        : base(contextFactory)
    {
    }

    /// <inheritdoc/>
    protected override async Task<CommandResult> ExecuteCoreAsync(IEnumerable<T> entities)
    {
        foreach (var entity in entities)
        {
            Context.Remove(entity);
        }

        await Context.SaveChangesAsync();

        return GetSuccessfulResult();
    }
}