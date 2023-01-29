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

public class CustomersViewModel : BindableBase, IViewModelBase<Customer>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CustomersViewModel"/> class.
    /// </summary>
    /// <param name="serviceProvider">Service provider.</param>
    public CustomersViewModel(IServiceProvider serviceProvider)
    {
        Model = serviceProvider.GetRequiredService<CustomersModel>();
        Model.PropertyChanged += (sender, args) => { RaisePropertyChanged(args.PropertyName); };
        SetLoadedItemIdCommand = new DelegateCommand<Guid?>(SetLoadedItemId);
        ResetNewItemCommand = new DelegateCommand(() =>
        {
            ResetNewItem();
            ResetNewOrder();
        });

        RefreshCommand = new DelegateCommand(async () => await ExecuteCommand(LoadDataAsync));
        AddCommand = new DelegateCommand(async () => await ExecuteCommand(AddDataAsync));
        UpdateCommand = new DelegateCommand(async () => await ExecuteCommand(UpdateDataAsync));
        DeleteCommand = new DelegateCommand<List<Customer>>(
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

    public DelegateCommand<Guid?> SetLoadedItemIdCommand { get; }

    public DelegateCommand RefreshCommand { get; }

    public DelegateCommand AddCommand { get; }

    public DelegateCommand UpdateCommand { get; }

    public DelegateCommand<List<Customer>> DeleteCommand { get; }

    public DelegateCommand RefreshOrdersCommand { get; }

    public DelegateCommand AddOrderCommand { get; }

    public DelegateCommand UpdateOrderCommand { get; }

    public DelegateCommand<List<Order>> DeleteOrderCommand { get; }

    public Customer NewItem { get; set; }

    public Customer SelectedItem { get; set; } = new ();

    public Order NewOrder { get; set; }

    public ObservableCollection<Customer> Items => Model.Items;

    public ObservableCollection<Order> Orders => Model.Orders;

    public bool ActionsBorderIsEnabled { get; set; }

    public bool OrdersActionsBorderIsEnabled { get; set; }

    private CustomersModel Model { get; }

    private Guid? LoadedItemId { get; set; }

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
        if (SelectedItem.User?.Login is null)
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

    private void SetLoadedItemId(Guid? id)
    {
        LoadedItemId = id;
    }

    private async Task LoadDataAsync()
    {
        var queryResult = await Task.Run(() => Model.GetItemsAsync(LoadedItemId));
        if (!queryResult.IsSuccessful)
        {
            MessageBox.Show(queryResult.ErrorMessage, @"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

            return;
        }

        Model.RefreshItems(queryResult.Data);
        if (LoadedItemId is null)
        {
            return;
        }

        if (Items.IsNullOrEmpty())
        {
            MessageBox.Show(
                @"Не заполнены данные о заказчике. Обратитесь к администратору",
                @"Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            return;
        }

        SelectedItem = Items.FirstOrDefault();
        RefreshOrdersCommand.Execute();
    }

    private async Task AddDataAsync()
    {
        bool anyNewItemPropertiesValuesIsNull = typeof(Customer).GetProperties()
            .Where(property => property.Name is not "Id" and not "Orders")
            .Any(property => property.GetValue(NewItem) is null);

        bool anyNewItemUserPropertiesValuesIsNull = typeof(User).GetProperties()
            .Where(property => property.Name is "Login")
            .Any(property => property.GetValue(NewItem.User) is null);

        if (anyNewItemPropertiesValuesIsNull || anyNewItemUserPropertiesValuesIsNull)
        {
            MessageBox.Show(@"Не все поля заполнены", @"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

            return;
        }

        var validateCommandResult = Model.ValidateCustomer(NewItem);
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
        var validateCommandResult = Model.ValidateCustomer(itemToUpdate);
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

    private async Task DeleteDataAsync(List<Customer> items)
    {
        bool haveAnyOrders = !Orders.IsNullOrEmpty();
        if (haveAnyOrders)
        {
            MessageBox.Show(
                @"У заказчика есть заказы. Необходимо удалить все заказы перед удалением заказчика.",
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
        SelectedItem = new Customer();
        MessageBox.Show(@"Успешно удалено", @"Успех", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async Task LoadOrdersDataAsync()
    {
        if (SelectedItem.User?.Login is null)
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
        if (SelectedItem.User?.Login is null)
        {
            MessageBox.Show(@"Не выбран заказчик", @"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

            return;
        }

        NewOrder.CustomerId = SelectedItem.Id;
        NewOrder.Status = OrderStatus.NotAccepted;
        NewOrder.CreationDate = DateTime.Now;
        bool anyNewOrderPropertiesValuesIsNull = typeof(Order).GetProperties()
            .Where(property => property.Name is "Title" or "Text")
            .Any(property => property.GetValue(NewOrder) is null);

        if (anyNewOrderPropertiesValuesIsNull)
        {
            MessageBox.Show(@"Не все поля заполнены", @"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

            return;
        }

        var validateCommandResult = Model.ValidateOrder(NewOrder);
        if (!validateCommandResult.IsSuccessful)
        {
            MessageBox.Show(validateCommandResult.ErrorMessage, @"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

            return;
        }

        var commandResult = await Task.Run(() => Model.AddOrderAsync(NewOrder));
        if (!commandResult.IsSuccessful)
        {
            MessageBox.Show(commandResult.ErrorMessage, @"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

            return;
        }

        Model.AddOrderToView(NewOrder);
        MessageBox.Show(@"Успешно добавлено", @"Успех", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async Task UpdateOrderDataAsync()
    {
        NewOrder.Id = NewItem.Id;
        NewOrder.CustomerId = SelectedItem.Id;
        var queryResult = Model.GetActualOrder(NewOrder);
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

    private async Task DeleteOrderDataAsync(List<Order> orders)
    {
        foreach (var order in orders)
        {
            order.Customer = null;
        }

        var commandResult = await Task.Run(() => Model.DeleteOrderAsync(orders));
        if (!commandResult.IsSuccessful)
        {
            MessageBox.Show(commandResult.ErrorMessage, @"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

            return;
        }

        Model.DeleteOrderFromView(orders);
        MessageBox.Show(@"Успешно удалено", @"Успех", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ResetNewItem()
    {
        NewItem = new Customer
        {
            User = new User(),
        };
    }

    private void ResetNewOrder()
    {
        NewOrder = new Order
        {
            StatementOfWork = new StatementOfWork(),
        };
    }
}