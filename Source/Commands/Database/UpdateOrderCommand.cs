using System.Threading.Tasks;
using Commands.Base;
using Dal;
using Dal.Entities;
using Microsoft.EntityFrameworkCore;

namespace Commands;

public class UpdateOrderCommand : AsyncCommandBase<Order>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateOrderCommand"/> class.
    /// </summary>
    /// <param name="contextFactory">Context factory.</param>
    public UpdateOrderCommand(IDbContextFactory<Context> contextFactory)
        : base(contextFactory)
    {
    }

    /// <inheritdoc/>
    protected override async Task<CommandResult> ExecuteCoreAsync(Order orderToUpdate)
    {
        var order = await Context.Orders.FirstOrDefaultAsync(order => order.Number == orderToUpdate.Number);
        order.Number = orderToUpdate.Number;
        order.Cost = orderToUpdate.Cost;
        order.Status = orderToUpdate.Status;
        order.Text = orderToUpdate.Text;
        order.Title = orderToUpdate.Title;
        order.DoneDate = orderToUpdate.DoneDate;
        order.StatementOfWork = orderToUpdate.StatementOfWork;
        Context.Orders.Update(order);
        await Context.SaveChangesAsync();

        return GetSuccessfulResult();
    }
}