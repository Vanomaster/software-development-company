using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using CleanModels;
using Dal.Entities;
using Gui.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Prism.Commands;
using Prism.Mvvm;

namespace Gui.ViewModels;

public class EmployeesViewModel : BindableBase, IViewModelBase<Employee>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmployeesViewModel"/> class.
    /// </summary>
    /// <param name="serviceProvider">Service provider.</param>
    public EmployeesViewModel(IServiceProvider serviceProvider)
    {
        Model = serviceProvider.GetRequiredService<EmployeesModel>();
        Model.PropertyChanged += (sender, args) => { RaisePropertyChanged(args.PropertyName); };
        ResetNewItemCommand = new DelegateCommand(() =>
        {
            ResetNewItem();
            ResetNewOrder();
        });

        RefreshCommand = new DelegateCommand(async () => await ExecuteCommand(LoadDataAsync));
        AddCommand = new DelegateCommand(async () => await ExecuteCommand(AddDataAsync));
        UpdateCommand = new DelegateCommand(async () => await ExecuteCommand(UpdateDataAsync));
        DeleteCommand = new DelegateCommand<List<Employee>>(
            async items => await ExecuteCommand(() => DeleteDataAsync(items)));

        RefreshOrdersCommand = new DelegateCommand(async () => await ExecuteCommand(LoadOrdersDataAsync));
        AddOrderCommand = new DelegateCommand(async () => await ExecuteCommand(AddOrderDataAsync));
        UpdateOrderCommand = new DelegateCommand(async () => await ExecuteCommand(UpdateOrderDataAsync));
        DeleteOrderCommand = new DelegateCommand<List<Order>>(
            async orders => await ExecuteCommand(() => DeleteOrderDataAsync(orders)));

        ResetNewItemCommand.Execute();
    }

    private delegate Task ExecuteCommandCallback();

    public DelegateCommand ResetNewItemCommand { get; }

    public DelegateCommand RefreshCommand { get; }

    public DelegateCommand AddCommand { get; }

    public DelegateCommand UpdateCommand { get; }

    public DelegateCommand<List<Employee>> DeleteCommand { get; }

    public DelegateCommand RefreshOrdersCommand { get; }

    public DelegateCommand AddOrderCommand { get; }

    public DelegateCommand UpdateOrderCommand { get; }

    public DelegateCommand<List<Order>> DeleteOrderCommand { get; }

    public Employee NewItem { get; set; }

    public Employee SelectedItem { get; set; } = new ();

    public Order NewOrder { get; set; }

    public ObservableCollection<Employee> Items => Model.Items;

    public ObservableCollection<Order> Orders => Model.Orders;

    public bool ActionsBorderIsEnabled { get; set; }

    public bool OrdersActionsBorderIsEnabled { get; set; }

    private EmployeesModel Model { get; }

    private async Task ExecuteCommand(ExecuteCommandCallback method)
    {
        ActionsBorderIsEnabled = false;
        OrdersActionsBorderIsEnabled = false;
        RaisePropertyChanged(nameof(ActionsBorderIsEnabled));
        RaisePropertyChanged(nameof(OrdersActionsBorderIsEnabled));
        await method.Invoke();
        string methodName = method.GetMethodInfo().Name;
        if (methodName.Contains("AddData") || methodName.Contains("UpdateData"))
        {
            ResetNewItem();
        }

        ActionsBorderIsEnabled = true;
        RaisePropertyChanged(nameof(ActionsBorderIsEnabled));
        if (SelectedItem.User?.Login is null || SelectedItem.User.Role == UserRole.Administrator)
        {
            return;
        }

        if (methodName.Contains("AddOrderData") || methodName.Contains("UpdateOrderData"))
        {
            ResetNewOrder();
        }

        OrdersActionsBorderIsEnabled = true;
        RaisePropertyChanged(nameof(OrdersActionsBorderIsEnabled));
    }

    private async Task LoadDataAsync()
    {
        var queryResult = await Task.Run(() => Model.GetItemsAsync());
        if (!queryResult.IsSuccessful)
        {
            MessageBox.Show(queryResult.ErrorMessage, @"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

            return;
        }

        Model.RefreshItems(queryResult.Data);
    }

    private async Task AddDataAsync()
    {
        bool anyNewItemPropertiesValuesIsNull = typeof(Employee).GetProperties()
            .Where(property => property.Name is not "Id" and not "Orders")
            .Any(property => property.GetValue(NewItem) is null);

        bool anyNewItemUserPropertiesValuesIsNull = typeof(User).GetProperties()
            .Where(property => property.Name is "Login")
            .Any(property => property.GetValue(NewItem.User) is null);

        bool anyNewItemPassportPropertiesValuesIsNull = typeof(Passport).GetProperties()
            .Where(property => property.Name is not "Employee")
            .Any(property => property.GetValue(NewItem.Passport) is null);

        if (anyNewItemPropertiesValuesIsNull || anyNewItemUserPropertiesValuesIsNull || anyNewItemPassportPropertiesValuesIsNull)
        {
            MessageBox.Show(@"Не все поля заполнены", @"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

            return;
        }

        var validateCommandResult = Model.ValidateEmployee(NewItem);
        if (!validateCommandResult.IsSuccessful)
        {
            MessageBox.Show(validateCommandResult.ErrorMessage, @"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

            return;
        }

        var commandResult = await Task.Run(() => Model.AddItemAsync(NewItem));
        if (!commandResult.IsSuccessful)
        {
            MessageBox.Show(commandResult.ErrorMessage, @"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

            return;
        }

        Model.AddItemToView(NewItem);
        MessageBox.Show(@"Успешно добавлено", @"Успех", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async Task UpdateDataAsync()
    {
        var queryResult = await Task.Run(() => Model.GetActualItem(NewItem));
        if (!queryResult.IsSuccessful)
        {
            MessageBox.Show(queryResult.ErrorMessage, @"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

            return;
        }

        var itemToUpdate = queryResult.Data;
        var validateCommandResult = Model.ValidateEmployee(itemToUpdate);
        if (!validateCommandResult.IsSuccessful)
        {
            MessageBox.Show(validateCommandResult.ErrorMessage, @"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

            return;
        }

        var commandResult = await Task.Run(() => Model.UpdateItemAsync(itemToUpdate));
        if (!commandResult.IsSuccessful)
        {
            MessageBox.Show(commandResult.ErrorMessage, @"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

            return;
        }

        Model.UpdateItemInView(itemToUpdate);
        MessageBox.Show(@"Успешно обновлено", @"Успех", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async Task DeleteDataAsync(List<Employee> items)
    {
        bool haveAnyOrders = !Orders.IsNullOrEmpty();
        if (haveAnyOrders)
        {
            MessageBox.Show(
                @"К сотруднику прикреплены заказы. Необходимо открепить все заказы перед удалением сотрудника.",
                @"Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            return;
        }

        var commandResult = await Task.Run(() => Model.DeleteItemAsync(items));
        if (!commandResult.IsSuccessful)
        {
            MessageBox.Show(commandResult.ErrorMessage, @"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

            return;
        }

        Model.DeleteItemFromView(items);
        SelectedItem = new Employee();
        MessageBox.Show(@"Успешно удалено", @"Успех", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async Task LoadOrdersDataAsync()
    {
        if (SelectedItem.User?.Login is null || SelectedItem.User.Role == UserRole.Administrator)
        {
            Model.RefreshOrders(new List<Order>());

            return;
        }

        var queryResult = await Task.Run(() => Model.GetOrdersAsync(SelectedItem.User.Login));
        if (!queryResult.IsSuccessful)
        {
            MessageBox.Show(queryResult.ErrorMessage, @"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

            return;
        }

        Model.RefreshOrders(queryResult.Data);
    }

    private async Task AddOrderDataAsync()
    {
        bool anyNewOrderPropertiesValuesIsNull = typeof(Order).GetProperties()
            .Where(property => property.Name is "Number")
            .Any(property => property.GetValue(NewOrder) is null);

        if (anyNewOrderPropertiesValuesIsNull)
        {
            MessageBox.Show(@"Не все поля заполнены", @"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

            return;
        }

        bool textIsNumber = int.TryParse(NewOrder.Number.ToString(), out _);
        if (!textIsNumber)
        {
            MessageBox.Show(
                @"Введён некорректный номер заказа. Номер заказа должен быть в виде положительного числа и не более 2 147 483 647.",
                @"Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            return;
        }

        bool isEmployeeAttachedToOrder = Orders.Any(order => order.Number == NewOrder.Number);
        if (isEmployeeAttachedToOrder)
        {
            MessageBox.Show(@"Заказ уже прикреплён к этому сотруднику", @"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

            return;
        }

        var queryResult = await Task.Run(() => Model.GetOrderByNumber(NewOrder.Number));
        if (!queryResult.IsSuccessful)
        {
            MessageBox.Show(queryResult.ErrorMessage, @"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

            return;
        }

        var commandResult = await Task.Run(() => Model.AttachOrderAsync(SelectedItem, NewOrder));
        if (!commandResult.IsSuccessful)
        {
            MessageBox.Show(commandResult.ErrorMessage, @"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

            return;
        }

        Model.AddOrderToView(queryResult.Data);
        MessageBox.Show(@"Успешно добавлено", @"Успех", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async Task UpdateOrderDataAsync()
    {
        NewOrder.Id = NewItem.Id;
        var queryResult = await Task.Run(() => Model.GetActualOrder(NewOrder));
        if (!queryResult.IsSuccessful)
        {
            MessageBox.Show(queryResult.ErrorMessage, @"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

            return;
        }

        var itemToUpdate = queryResult.Data;
        var validateCommandResult = Model.ValidateOrder(itemToUpdate);
        if (!validateCommandResult.IsSuccessful)
        {
            MessageBox.Show(validateCommandResult.ErrorMessage, @"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

            return;
        }

        var commandResult = await Task.Run(() => Model.UpdateOrderAsync(itemToUpdate));
        if (!commandResult.IsSuccessful)
        {
            MessageBox.Show(commandResult.ErrorMessage, @"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

            return;
        }

        Model.UpdateOrderInView(itemToUpdate);
        MessageBox.Show(@"Успешно обновлено", @"Успех", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async Task DeleteOrderDataAsync(List<Order> items)
    {
        var commandResult = await Task.Run(() => Model.DettachOrderAsync(SelectedItem, items));
        if (!commandResult.IsSuccessful)
        {
            MessageBox.Show(commandResult.ErrorMessage, @"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

            return;
        }

        Model.DeleteOrderFromView(items);
        MessageBox.Show(@"Успешно удалено", @"Успех", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ResetNewItem()
    {
        NewItem = new Employee
        {
            User = new User(),
            Passport = new Passport(),
        };
    }

    private void ResetNewOrder()
    {
        NewOrder = new Order
        {
            Customer = new Customer
            {
                User = new User(),
            },
            StatementOfWork = new StatementOfWork(),
        };
    }
}