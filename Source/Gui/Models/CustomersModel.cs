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
using Prism.Mvvm;
using Queries;
using Queries.Base;

namespace Gui.Models;

public class CustomersModel : BindableBase
{
    public readonly ObservableCollection<Customer> Items = new ();

    public readonly ObservableCollection<Order> Orders = new ();

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomersModel"/> class.
    /// </summary>
    /// <param name="serviceProvider">Service provider.</param>
    public CustomersModel(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    /// <summary>
    /// Service provider.
    /// </summary>
    private IServiceProvider ServiceProvider { get; }

    public async Task<QueryResult<List<Customer>>> GetItemsAsync(Guid? itemId)
    {
        var query = ServiceProvider.GetRequiredService<CustomersQuery>();
        var queryResult = itemId.HasValue
            ? await query.ExecuteAsync(new List<Guid> { itemId.Value })
            : await query.ExecuteAsync();

        if (!queryResult.IsSuccessful)
        {
            return new QueryResult<List<Customer>>(queryResult.ErrorMessage);
        }

        return new QueryResult<List<Customer>>(queryResult.Data);
    }

    public void RefreshItems(List<Customer> newItems)
    {
        var newItemsOrderedByLogin = newItems.OrderBy(item => item.User.Login);
        Items.Clear();
        Items.AddRange(newItemsOrderedByLogin);
        RaisePropertyChanged("Items"); // TODO nameof
    }

    public CommandResult ValidateCustomer(Customer customer)
    {
        var validationErrors = string.Empty;
        var dataValidator = ServiceProvider.GetRequiredService<DataValidator>();
        var firstNameValidationCommandResult = dataValidator.IsValidFirstName(customer.FirstName);
        if (!firstNameValidationCommandResult.IsSuccessful)
        {
            validationErrors += firstNameValidationCommandResult.ErrorMessage + "\n";
        }

        var lastNameValidationCommandResult = dataValidator.IsValidLastName(customer.LastName);
        if (!lastNameValidationCommandResult.IsSuccessful)
        {
            validationErrors += lastNameValidationCommandResult.ErrorMessage + "\n";
        }

        var patronymicValidationCommandResult = dataValidator.IsValidPatronymic(customer.Patronymic);
        if (!patronymicValidationCommandResult.IsSuccessful)
        {
            validationErrors += patronymicValidationCommandResult.ErrorMessage + "\n";
        }

        var emailValidationCommandResult = dataValidator.IsValidEmail(customer.EmailAddress);
        if (!emailValidationCommandResult.IsSuccessful)
        {
            validationErrors += emailValidationCommandResult.ErrorMessage + "\n";
        }

        if (validationErrors != string.Empty)
        {
            return new CommandResult(validationErrors);
        }

        return new CommandResult();
    }

    public async Task<CommandResult> AddItemAsync(Customer itemToAdd)
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

        if (user.Role != UserRole.Customer)
        {
            return new CommandResult(@"Пользователь с таким логином не является заказчиком");
        }

        if (Items.Any(item => item.Id == user.Id))
        {
            return new CommandResult(@"Данные о пользователе с таким логином уже присутствуют в таблице заказчиков");
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

    public void AddItemToView(Customer itemToAdd)
    {
        Items.Add(itemToAdd);
        RaisePropertyChanged("Items");
    }

    public async Task<CommandResult<Customer>> GetActualItem(Customer itemToUpdate)
    {
        var item = Items.SingleOrDefault(item => item.Id == itemToUpdate.Id);
        if (item == null)
        {
            return new CommandResult<Customer>("Неопознанная ошибка");
        }

        var query = ServiceProvider.GetRequiredService<UsersQueryByLogin>();
        var queryResult = await query.ExecuteAsync(new List<string> { item.User.Login });
        if (!queryResult.IsSuccessful)
        {
            return new CommandResult<Customer>(queryResult.ErrorMessage);
        }

        var user = queryResult.Data.FirstOrDefault()!;
        var actualItem = new Customer
        {
            Id = itemToUpdate.Id,
            LastName = string.IsNullOrWhiteSpace(itemToUpdate.LastName)
                ? item.LastName
                : itemToUpdate.LastName,
            FirstName = string.IsNullOrWhiteSpace(itemToUpdate.FirstName)
                ? item.FirstName
                : itemToUpdate.FirstName,
            Patronymic = string.IsNullOrWhiteSpace(itemToUpdate.Patronymic)
                ? item.Patronymic
                : itemToUpdate.Patronymic,
            Gender = itemToUpdate.Gender == 0
                ? item.Gender
                : itemToUpdate.Gender,
            EmailAddress = string.IsNullOrWhiteSpace(itemToUpdate.EmailAddress)
                ? item.EmailAddress
                : itemToUpdate.EmailAddress,
            User = user,
        };

        return new CommandResult<Customer>(actualItem);
    }

    public async Task<CommandResult> UpdateItemAsync(Customer itemToUpdate)
    {
        var command = ServiceProvider.GetRequiredService<AddOrUpdateEntityCommand>();
        var commandResult = await command.ExecuteAsync(new List<IEntity> { itemToUpdate });
        if (!commandResult.IsSuccessful)
        {
            return new CommandResult(commandResult.ErrorMessage);
        }

        return new CommandResult();
    }

    public void UpdateItemInView(Customer itemToUpdate)
    {
        var item = Items.SingleOrDefault(item => item.Id == itemToUpdate.Id);
        Items.Remove(item);
        Items.Add(itemToUpdate);
        RaisePropertyChanged("Items");
    }

    public async Task<CommandResult> DeleteItemAsync(List<Customer> itemsToDelete)
    {
        var command = ServiceProvider.GetRequiredService<DeleteEntityCommand>();
        var commandResult = await command.ExecuteAsync(itemsToDelete);
        if (!commandResult.IsSuccessful)
        {
            return new CommandResult(commandResult.ErrorMessage);
        }

        return new CommandResult();
    }

    public void DeleteItemFromView(List<Customer> itemsToDelete)
    {
        foreach (var item in itemsToDelete)
        {
            Items.Remove(item);
        }

        RaisePropertyChanged("Items");
    }

    public async Task<QueryResult<List<Order>>> GetOrdersAsync(string customerLogin)
    {
        var query = ServiceProvider.GetRequiredService<OrdersQueryByCustomerLogin>();
        var queryResult = await query.ExecuteAsync(customerLogin);
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
        var firstNameValidationCommandResult = dataValidator.IsValidTitle(order.Title, @"заказа");
        if (!firstNameValidationCommandResult.IsSuccessful)
        {
            validationErrors += firstNameValidationCommandResult.ErrorMessage + "\n";
        }

        var lastNameValidationCommandResult = dataValidator.IsValidText(order.Text, @"заказа");
        if (!lastNameValidationCommandResult.IsSuccessful)
        {
            validationErrors += lastNameValidationCommandResult.ErrorMessage + "\n";
        }

        var patronymicValidationCommandResult = dataValidator.IsValidCost(order.Cost);
        if (!patronymicValidationCommandResult.IsSuccessful)
        {
            validationErrors += patronymicValidationCommandResult.ErrorMessage + "\n";
        }

        if (validationErrors != string.Empty)
        {
            return new CommandResult(validationErrors);
        }

        return new CommandResult();
    }

    public async Task<CommandResult> AddOrderAsync(Order itemToAdd)
    {
        var customer = itemToAdd.Customer;
        itemToAdd.Customer = null!;
        itemToAdd.StatementOfWork = null!;
        var addCommand = ServiceProvider.GetRequiredService<AddEntityCommand>();
        var addCommandResult = await addCommand.ExecuteAsync(new List<IEntity> { itemToAdd });
        if (!addCommandResult.IsSuccessful)
        {
            return new CommandResult(addCommandResult.ErrorMessage);
        }

        itemToAdd.Customer = customer;

        return new CommandResult();
    }

    public void AddOrderToView(Order itemToAdd)
    {
        Orders.Add(itemToAdd);
        RaisePropertyChanged("Orders");
    }

    public CommandResult<Order> GetActualOrder(Order itemToUpdate)
    {
        var item = Orders.SingleOrDefault(item => item.Id == itemToUpdate.Id);
        if (item == null)
        {
            return new CommandResult<Order>("Неопознанная ошибка");
        }

        var actualItem = new Order
        {
            Id = itemToUpdate.Id,
            CustomerId = itemToUpdate.CustomerId,
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
            DoneDate = item.DoneDate,
            StatementOfWork = item.StatementOfWork?.Id == Guid.Empty ? null : item.StatementOfWork,
            Customer = null,
            /*StatementOfWork = new StatementOfWork
            {
                Title = string.IsNullOrWhiteSpace(itemToUpdate.StatementOfWork.Title)
                    ? item.StatementOfWork.Title
                    : itemToUpdate.StatementOfWork.Title,
                Text = string.IsNullOrWhiteSpace(itemToUpdate.StatementOfWork.Text)
                    ? item.StatementOfWork.Text
                    : itemToUpdate.StatementOfWork.Text,
                Status = itemToUpdate.StatementOfWork.Status == 0
                    ? item.StatementOfWork.Status
                    : itemToUpdate.StatementOfWork.Status,
                DoneDate = item.StatementOfWork.DoneDate,
            },*/
        };

        return new CommandResult<Order>(actualItem);
    }

    public async Task<CommandResult> UpdateOrderAsync(Order itemToUpdate)
    {
        var command = ServiceProvider.GetRequiredService<AddOrUpdateEntityCommand>();
        var commandResult = await command.ExecuteAsync(new List<IEntity> { itemToUpdate });
        if (!commandResult.IsSuccessful)
        {
            return new CommandResult(commandResult.ErrorMessage);
        }

        return new CommandResult();
    }

    public void UpdateOrderInView(Order itemToUpdate)
    {
        itemToUpdate.StatementOfWork ??= new StatementOfWork();
        var item = Orders.SingleOrDefault(item => item.Id == itemToUpdate.Id);
        Orders.Remove(item);
        Orders.Add(itemToUpdate);
        RaisePropertyChanged("Orders");
    }

    public async Task<CommandResult> DeleteOrderAsync(List<Order> itemsToDelete)
    {
        var command = ServiceProvider.GetRequiredService<DeleteEntityCommand>();
        var commandResult = await command.ExecuteAsync(itemsToDelete);
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