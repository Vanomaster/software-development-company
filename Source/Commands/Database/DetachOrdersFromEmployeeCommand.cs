using System.Linq;
using System.Threading.Tasks;
using CleanModels;
using Commands.Base;
using Dal;
using Microsoft.EntityFrameworkCore;

namespace Commands;

public class DetachOrdersFromEmployeeCommand : AsyncCommandBase<DetachOrdersFromEmployeeModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DetachOrdersFromEmployeeCommand"/> class.
    /// </summary>
    /// <param name="contextFactory">Context factory.</param>
    public DetachOrdersFromEmployeeCommand(IDbContextFactory<Context> contextFactory)
        : base(contextFactory)
    {
    }

    /// <inheritdoc/>
    protected override async Task<CommandResult> ExecuteCoreAsync(DetachOrdersFromEmployeeModel model)
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

        employee.Orders.RemoveAll(order => model.Orders
            .Select(orderToDetach => orderToDetach.Number)
            .Contains(order.Number));

        await Context.SaveChangesAsync();

        return GetSuccessfulResult();
    }
}