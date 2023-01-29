using System.Linq;
using System.Threading.Tasks;
using Commands.Base;
using Dal;
using Microsoft.EntityFrameworkCore;

namespace Commands.Database;

public class TestConnectionCommand : AsyncCommandBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestConnectionCommand"/> class.
    /// </summary>
    /// <param name="contextFactory">Context factory.</param>
    public TestConnectionCommand(IDbContextFactory<Context> contextFactory)
        : base(contextFactory)
    {
    }

    /// <inheritdoc/>
    protected override async Task<CommandResult> ExecuteCoreAsync()
    {
        var users = await Context.Users
            .Where(user => user.Login == string.Empty)
            .ToListAsync();

        users.Clear();
        await Context.SaveChangesAsync();

        return GetSuccessfulResult();
    }
}