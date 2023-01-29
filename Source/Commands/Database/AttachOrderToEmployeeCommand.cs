using System.Linq;
using System.Threading.Tasks;
using CleanModels;
using Commands.Base;
using Dal;
using Microsoft.EntityFrameworkCore;

namespace Commands;

public class AttachOrderToEmployeeCommand : AsyncCommandBase<AttachOrderToEmployeeModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AttachOrderToEmployeeCommand"/> class.
    /// </summary>
    /// <param name="contextFactory">Context factory.</param>
    public AttachOrderToEmployeeCommand(IDbContextFactory<Context> contextFactory)
        : base(contextFactory)
    {
    }

    /// <inheritdoc/>
    protected override async Task<CommandResult> ExecuteCoreAsync(AttachOrderToEmployeeModel model)
    {
        var employee = await Context.Employees
            .Where(employee => employee.Id == model.Employee.Id)
            .Include(entity => entity.Orders)
            .Include(entity => entity.User)
            .FirstOrDefaultAsync();

        if (employee == null)
        {
            return GetFailedResult(@"Сотрудник не найден");
        }

        var order = await Context.Orders
            .Where(order => order.Number == model.Order.Number)
            .Include(entity => entity.Employees)
            .Include(entity => entity.Customer.User)
            .FirstOrDefaultAsync();

        if (order == null)
        {
            return GetFailedResult(@"Заказ с таким номером не найден");
        }

        employee.Orders.Add(order);
        await Context.SaveChangesAsync();

        return GetSuccessfulResult();
    }
}