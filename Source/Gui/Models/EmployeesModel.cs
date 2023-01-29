using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CleanModels;
using Commands;
using Commands.Base;
using Dal.Entities;
using Gui.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Prism.Mvvm;
using Queries;
using Queries.Base;

namespace Gui.Models;

public class EmployeesModel : BindableBase
{
    public readonly ObservableCollection<Employee> Items = new ();

    public readonly ObservableCollection<Order> Orders = new ();

    /// <summary>
    /// Initializes a new instance of the <see cref="EmployeesModel"/> class.
    /// </summary>
    /// <param name="serviceProvider">Service provider.</param>
    public EmployeesModel(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    /// <summary>
    /// Service provider.
    /// </summary>
    private IServiceProvider ServiceProvider { get; }

    public async Task<QueryResult<List<Employee>>> GetItemsAsync()
    {
        var query = ServiceProvider.GetRequiredService<EmployeesQuery>();
        var queryResult = await query.ExecuteAsync();
        if (!queryResult.IsSuccessful)
        {
            return new QueryResult<List<Employee>>(queryResult.ErrorMessage);
        }

        return new QueryResult<List<Employee>>(queryResult.Data);
    }

    public void RefreshItems(List<Employee> newItems)
    {
        var newItemsOrderedByLogin = newItems.OrderBy(item => item.User.Login);
        Items.Clear();
        Items.AddRange(newItemsOrderedByLogin);
        RaisePropertyChanged("Items");
    }

    public CommandResult ValidateEmployee(Employee employee)
    {
        var validationErrors = string.Empty;
        var dataValidator = ServiceProvider.GetRequiredService<DataValidator>();
        var passportSeriesValidationCommandResult = dataValidator.IsValidPassportSeries(employee.PassportSeries);
        if (!passportSeriesValidationCommandResult.IsSuccessful)
        {
            validationErrors += passportSeriesValidationCommandResult.ErrorMessage + "\n";
        }

        var passportNumberValidationCommandResult = dataValidator.IsValidPassportNumber(employee.PassportNumber);
        if (!passportNumberValidationCommandResult.IsSuccessful)
        {
            validationErrors += passportNumberValidationCommandResult.ErrorMessage + "\n";
        }

        var emailValidationCommandResult = dataValidator.IsValidEmail(employee.EmailAddress);
        if (!emailValidationCommandResult.IsSuccessful)
        {
            validationErrors += emailValidationCommandResult.ErrorMessage + "\n";
        }

        var lastNameValidationCommandResult = dataValidator.IsValidLastName(employee.Passport.LastName);
        if (!lastNameValidationCommandResult.IsSuccessful)
        {
            validationErrors += lastNameValidationCommandResult.ErrorMessage + "\n";
        }

        var firstNameValidationCommandResult = dataValidator.IsValidFirstName(employee.Passport.FirstName);
        if (!firstNameValidationCommandResult.IsSuccessful)
        {
            validationErrors += firstNameValidationCommandResult.ErrorMessage + "\n";
        }

        var patronymicValidationCommandResult = dataValidator.IsValidPatronymic(employee.Passport.Patronymic);
        if (!patronymicValidationCommandResult.IsSuccessful)
        {
            validationErrors += patronymicValidationCommandResult.ErrorMessage + "\n";
        }

        var birthDateValidationCommandResult = dataValidator.IsValidBirthDate(employee.Passport.BirthDate);
        if (!birthDateValidationCommandResult.IsSuccessful)
        {
            validationErrors += birthDateValidationCommandResult.ErrorMessage + "\n";
        }

        var residenceValidationCommandResult = dataValidator.IsValidResidence(employee.Passport.Residence);
        if (!residenceValidationCommandResult.IsSuccessful)
        {
            validationErrors += residenceValidationCommandResult.ErrorMessage + "\n";
        }

        if (validationErrors != string.Empty)
        {
            return new CommandResult(validationErrors);
        }

        return new CommandResult();
    }

    public async Task<CommandResult> AddItemAsync(Employee itemToAdd)
    {
        var query = ServiceProvider.GetRequiredService<UsersQueryByLogin>();
        var queryResult = await query.ExecuteAsync(new List<string> { itemToAdd.User.Login });
        if (!queryResult.IsSuccessful)
        {
            return new CommandResult(queryResult.ErrorMessage);
        }

        var user = queryResult.Data.FirstOrDefault();
        if (user == null)
        {
            return new CommandResult(@"Пользователь с таким логином не найден");
        }

        if (user.Role != UserRole.Employee && user.Role != UserRole.Administrator)
        {
            return new CommandResult(@"Пользователь с таким логином не является сотрудником");
        }

        if (Items.Any(item => item.Id == user.Id))
        {
            return new CommandResult(@"Данные о пользователе с таким логином уже присутствуют в таблице сотрудников");
        }

        itemToAdd.Id = user.Id;
        itemToAdd.User = null!;
        var addCommand = ServiceProvider.GetRequiredService<AddEntityCommand>();
        var addCommandResult = await addCommand.ExecuteAsync(new List<IEntity> { itemToAdd });
        if (!addCommandResult.IsSuccessful)
        {
            return new CommandResult(addCommandResult.ErrorMessage);
        }

        itemToAdd.User = user;

        return new CommandResult();
    }

    public void AddItemToView(Employee itemToAdd)
    {
        Items.Add(itemToAdd);
        RaisePropertyChanged("Items");
    }

    public async Task<CommandResult<Employee>> GetActualItem(Employee itemToUpdate)
    {
        var item = Items.SingleOrDefault(item => item.Id == itemToUpdate.Id);
        if (item == null)
        {
            return new CommandResult<Employee>("Неопознанная ошибка");
        }

        var query = ServiceProvider.GetRequiredService<UsersQueryByLogin>();
        var queryResult = await query.ExecuteAsync(new List<string> { item.User.Login });
        if (!queryResult.IsSuccessful)
        {
            return new CommandResult<Employee>(queryResult.ErrorMessage);
        }

        var user = queryResult.Data.FirstOrDefault()!;
        var actualItem = new Employee
        {
            Id = itemToUpdate.Id,
            PassportSeries = itemToUpdate.PassportSeries == 0
                ? item.PassportSeries
                : itemToUpdate.PassportSeries,
            PassportNumber = itemToUpdate.PassportNumber == 0
                ? item.PassportNumber
                : itemToUpdate.PassportNumber,
            JobPosition = itemToUpdate.JobPosition == 0
                ? item.JobPosition
                : itemToUpdate.JobPosition,
            EmailAddress = string.IsNullOrWhiteSpace(itemToUpdate.EmailAddress)
                ? item.EmailAddress
                : itemToUpdate.EmailAddress,
            User = user,
            Passport = new Passport
            {
                Series = itemToUpdate.PassportSeries == 0
                    ? item.PassportSeries
                    : itemToUpdate.PassportSeries,
                Number = itemToUpdate.PassportNumber == 0
                    ? item.PassportNumber
                    : itemToUpdate.PassportNumber,
                LastName = string.IsNullOrWhiteSpace(itemToUpdate.Passport.LastName)
                    ? item.Passport.LastName
                    : itemToUpdate.Passport.LastName,
                FirstName = string.IsNullOrWhiteSpace(itemToUpdate.Passport.FirstName)
                    ? item.Passport.FirstName
                    : itemToUpdate.Passport.FirstName,
                Patronymic = string.IsNullOrWhiteSpace(itemToUpdate.Passport.Patronymic)
                    ? item.Passport.Patronymic
                    : itemToUpdate.Passport.Patronymic,
                Gender = itemToUpdate.Passport.Gender == 0
                    ? item.Passport.Gender
                    : itemToUpdate.Passport.Gender,
                BirthDate = itemToUpdate.Passport.BirthDate == default
                    ? item.Passport.BirthDate
                    : itemToUpdate.Passport.BirthDate,
                Residence = string.IsNullOrWhiteSpace(itemToUpdate.Passport.Residence)
                    ? item.Passport.Residence
                    : itemToUpdate.Passport.Residence,
            },
        };

        return new CommandResult<Employee>(actualItem);
    }

    public async Task<CommandResult> UpdateItemAsync(Employee itemToUpdate)
    {
        var command = ServiceProvider.GetRequiredService<AddOrUpdateEntityCommand>();
        var commandResult = await command.ExecuteAsync(new List<IEntity> { itemToUpdate });
        if (!commandResult.IsSuccessful)
        {
            return new CommandResult(commandResult.ErrorMessage);
        }

        return new CommandResult();
    }

    public void UpdateItemInView(Employee itemToUpdate)
    {
        var item = Items.SingleOrDefault(item => item.Id == itemToUpdate.Id);
        Items.Remove(item);
        Items.Add(itemToUpdate);
        RaisePropertyChanged("Items");
    }

    public async Task<CommandResult> DeleteItemAsync(List<Employee> itemsToDelete)
    {
        var command = ServiceProvider.GetRequiredService<DeleteEmployeeIdentityEntityCommand>();
        var commandResult = await command.ExecuteAsync(itemsToDelete);
        if (!commandResult.IsSuccessful)
        {
            return new CommandResult(commandResult.ErrorMessage);
        }

        return new CommandResult();
    }

    public void DeleteItemFromView(List<Employee> itemsToDelete)
    {
        foreach (var item in itemsToDelete)
        {
            Items.Remove(item);
        }

        RaisePropertyChanged("Items");
    }

    public async Task<QueryResult<List<Order>>> GetOrdersAsync(string login)
    {
        var query = ServiceProvider.GetRequiredService<OrdersQueryByEmployeeLogin>();
        var queryResult = await query.ExecuteAsync(login);
        if (!queryResult.IsSuccessful)
        {
            return new QueryResult<List<Order>>(queryResult.ErrorMessage);
        }

        return new QueryResult<List<Order>>(queryResult.Data);
    }

    public void RefreshOrders(List<Order> newItems)
    {
        var newItemsOrderedByNumber = newItems.OrderByDescending(item => item.Number);
        Orders.Clear();
        Orders.AddRange(newItemsOrderedByNumber);
        RaisePropertyChanged("Orders");
    }

    public CommandResult ValidateOrder(Order order)
    {
        var validationErrors = string.Empty;
        var dataValidator = ServiceProvider.GetRequiredService<DataValidator>();
        var titleValidationCommandResult = dataValidator.IsValidTitle(order.Title, @"заказа");
        if (!titleValidationCommandResult.IsSuccessful)
        {
            validationErrors += titleValidationCommandResult.ErrorMessage + "\n";
        }

        var textValidationCommandResult = dataValidator.IsValidText(order.Text, @"заказа");
        if (!textValidationCommandResult.IsSuccessful)
        {
            validationErrors += textValidationCommandResult.ErrorMessage + "\n";
        }

        var costValidationCommandResult = dataValidator.IsValidCost(order.Cost);
        if (!costValidationCommandResult.IsSuccessful)
        {
            validationErrors += costValidationCommandResult.ErrorMessage + "\n";
        }

        var swTitleValidationCommandResult = dataValidator.IsValidTitle(order.StatementOfWork.Title, "ТЗ");
        if (!swTitleValidationCommandResult.IsSuccessful)
        {
            validationErrors += swTitleValidationCommandResult.ErrorMessage + "\n";
        }

        var swTextValidationCommandResult = dataValidator.IsValidText(order.StatementOfWork.Text, "ТЗ");
        if (!swTextValidationCommandResult.IsSuccessful)
        {
            validationErrors += swTextValidationCommandResult.ErrorMessage + "\n";
        }

        if (validationErrors != string.Empty)
        {
            return new CommandResult(validationErrors);
        }

        return new CommandResult();
    }

    public async Task<QueryResult<Order>> GetOrderByNumber(int number)
    {
        var query = ServiceProvider.GetRequiredService<OrdersQueryByNumber>();
        var queryResult = await query.ExecuteAsync(new List<int> { number });
        if (!queryResult.IsSuccessful)
        {
            return new QueryResult<Order>(queryResult.ErrorMessage);
        }

        var order = queryResult.Data.FirstOrDefault();
        if (order == null)
        {
            return new QueryResult<Order>("Заказ с таким номером не найден");
        }

        return new QueryResult<Order>(order);
    }

    public async Task<CommandResult> AttachOrderAsync(Employee selectedItem, Order itemToAdd)
    {
        var model = new AttachOrderToEmployeeModel
        {
            Employee = selectedItem,
            Order = itemToAdd,
        };

        var command = ServiceProvider.GetRequiredService<AttachOrderToEmployeeCommand>();
        var commandResult = await command.ExecuteAsync(model);
        if (!commandResult.IsSuccessful)
        {
            return new CommandResult(commandResult.ErrorMessage);
        }

        return new CommandResult();
    }

    public void AddOrderToView(Order itemToAdd)
    {
        Orders.Add(itemToAdd);
        RaisePropertyChanged("Orders");
    }

    public async Task<CommandResult<Order>> GetActualOrder(Order itemToUpdate)
    {
        var query = ServiceProvider.GetRequiredService<OrdersQuery>();
        var queryResult = await query.ExecuteAsync(new List<Guid> { itemToUpdate.Id });
        if (!queryResult.IsSuccessful)
        {
            return new CommandResult<Order>(queryResult.ErrorMessage);
        }

        var item = queryResult.Data.FirstOrDefault();
        if (item == null)
        {
            return new CommandResult<Order>("Заказ не найден");
        }

        var actualItem = new Order
        {
            Id = itemToUpdate.Id,
            Number = item.Number,
            Title = string.IsNullOrWhiteSpace(itemToUpdate.Title)
                ? item.Title
                : itemToUpdate.Title,
            Text = string.IsNullOrWhiteSpace(itemToUpdate.Text)
                ? item.Text
                : itemToUpdate.Text,
            Cost = itemToUpdate.Cost ?? item.Cost,
            Status = itemToUpdate.Status == 0
                ? item.Status
                : itemToUpdate.Status,
            CreationDate = item.CreationDate,
            DoneDate = itemToUpdate.DoneDate ?? item.DoneDate,
            Customer = item.Customer,
            Employees = item.Employees,
        };

        if (item.StatementOfWork == null)
        {
            var newSwProperties = typeof(StatementOfWork).GetProperties()
                .Where(property => property.Name is not "Id" and not "DoneDate" and not "Status" and not "OrderId" and not "Order");

            foreach (var property in newSwProperties)
            {
                object? value = property.GetValue(itemToUpdate.StatementOfWork);
                if (string.IsNullOrWhiteSpace(value?.ToString()))
                {
                    property.SetValue(itemToUpdate.StatementOfWork, "_");
                }
            }

            actualItem.StatementOfWork = new StatementOfWork
            {
                Id = Guid.Empty,
                Title = itemToUpdate.StatementOfWork.Title,
                Text = itemToUpdate.StatementOfWork.Text,
                Status = itemToUpdate.StatementOfWork.Status,
                DoneDate = itemToUpdate.StatementOfWork.DoneDate,
            };
        }

        if (item.StatementOfWork != null)
        {
            actualItem.StatementOfWork = new StatementOfWork
            {
                Id = item.StatementOfWork.Id,
                Title = string.IsNullOrWhiteSpace(itemToUpdate.StatementOfWork.Title)
                    ? item.StatementOfWork.Title
                    : itemToUpdate.StatementOfWork.Title,
                Text = string.IsNullOrWhiteSpace(itemToUpdate.StatementOfWork.Text)
                    ? item.StatementOfWork.Text
                    : itemToUpdate.StatementOfWork.Text,
                Status = itemToUpdate.StatementOfWork.Status == 0
                    ? item.StatementOfWork.Status
                    : itemToUpdate.StatementOfWork.Status,
                DoneDate = itemToUpdate.StatementOfWork.DoneDate ?? item.StatementOfWork.DoneDate,
            };
        }

        if (actualItem.StatementOfWork?.Status == 0)
        {
            actualItem.StatementOfWork.Status = StatementOfWorkStatus.NotDone;
        }

        if (item.StatementOfWork?.Status != StatementOfWorkStatus.Done
            && actualItem.StatementOfWork?.Status == StatementOfWorkStatus.Done)
        {
            actualItem.StatementOfWork.DoneDate = DateTime.Now;
        }

        if (actualItem.Employees.IsNullOrEmpty() && actualItem.Status != item.Status)
        {
            return new CommandResult<Order>("Необходимо прикрепить заказ к сотруднику");
        }

        if (actualItem.StatementOfWork?.Status != StatementOfWorkStatus.Done
            && item.Status == OrderStatus.StatementOfWorkDevelopment
            && actualItem.Status != item.Status)
        {
            return new CommandResult<Order>("Необходимо завершить работу над ТЗ");
        }

        if (actualItem.Cost is null or 0
            && item.Status == OrderStatus.StatementOfWorkDevelopment
            && actualItem.Status != item.Status)
        {
            return new CommandResult<Order>("Необходимо определить стоимость разработки");
        }

        int itemStatusDifference = (int)actualItem.Status - (int)item.Status;
        if (itemStatusDifference > 1)
        {
            return new CommandResult<Order>("Необходимо пройти по порядку все этапы разработки программного обеспечения");
        }

        if (actualItem.Status == OrderStatus.Done)
        {
            actualItem.DoneDate = DateTime.Now;
        }

        return new CommandResult<Order>(actualItem);
    }

    public async Task<CommandResult> UpdateOrderAsync(Order itemToUpdate)
    {
        var command = ServiceProvider.GetRequiredService<UpdateOrderCommand>();
        var commandResult = await command.ExecuteAsync(itemToUpdate);
        if (!commandResult.IsSuccessful)
        {
            return new CommandResult(commandResult.ErrorMessage);
        }

        return new CommandResult();
    }

    public void UpdateOrderInView(Order itemToUpdate)
    {
        var item = Orders.SingleOrDefault(item => item.Id == itemToUpdate.Id);
        Orders.Remove(item);
        Orders.Add(itemToUpdate);
        RaisePropertyChanged("Orders");
    }

    public async Task<CommandResult> DettachOrderAsync(Employee selectedItem, List<Order> itemsToDelete)
    {
        var model = new DetachOrdersFromEmployeeModel
        {
            Employee = selectedItem,
            Orders = itemsToDelete,
        };

        var command = ServiceProvider.GetRequiredService<DetachOrdersFromEmployeeCommand>();
        var commandResult = await command.ExecuteAsync(model);
        if (!commandResult.IsSuccessful)
        {
            return new CommandResult(commandResult.ErrorMessage);
        }

        return new CommandResult();
    }

    public void DeleteOrderFromView(List<Order> itemsToDelete)
    {
        foreach (var item in itemsToDelete)
        {
            Orders.Remove(item);
        }

        RaisePropertyChanged("Orders");
    }
}