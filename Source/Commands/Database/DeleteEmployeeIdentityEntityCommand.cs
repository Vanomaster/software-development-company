using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Commands.Base;
using Dal;
using Dal.Entities;
using Microsoft.EntityFrameworkCore;

namespace Commands;

public class DeleteEmployeeIdentityEntityCommand : AsyncCommandBase<IEnumerable<Employee>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteEmployeeIdentityEntityCommand"/> class.
    /// </summary>
    /// <param name="contextFactory">Context factory.</param>
    public DeleteEmployeeIdentityEntityCommand(IDbContextFactory<Context> contextFactory)
        : base(contextFactory)
    {
    }

    /// <inheritdoc/>
    protected override async Task<CommandResult> ExecuteCoreAsync(IEnumerable<Employee> entities)
    {
        Context.Employees.RemoveRange(entities);
        Context.Passports.RemoveRange(entities.Select(entity => entity.Passport));
        await Context.SaveChangesAsync();

        return GetSuccessfulResult();
    }
}