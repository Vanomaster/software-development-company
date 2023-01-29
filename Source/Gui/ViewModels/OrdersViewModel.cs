using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Dal.Entities;
using Gui.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Prism.Commands;
using Prism.Mvvm;

namespace Gui.ViewModels;

public class OrdersViewModel : BindableBase, IViewModelBase<Order>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OrdersViewModel"/> class.
    /// </summary>
    /// <param name="serviceProvider">Service provider.</param>
    public OrdersViewModel(IServiceProvider serviceProvider)
    {
        Model = serviceProvider.GetRequiredService<OrdersModel>();
        Model.PropertyChanged += (sender, args) => { RaisePropertyChanged(args.PropertyName); };
        ResetNewItemCommand = new DelegateCommand(() =>
        {
            ResetNewItem();
            ResetNewEmployee();
        });

        RefreshCommand = new DelegateCommand(async () => await ExecuteCommand(LoadDataAsync));
        UpdateCommand = new DelegateCommand(async () => await ExecuteCommand(UpdateDataAsync));

        RefreshEmployeesCommand = new DelegateCommand(async () => await ExecuteCommand(LoadEmployeesDataAsync));
        AddEmployeeCommand = new DelegateCommand(async () => await ExecuteCommand(AddEmployeeDataAsync));
        UpdateEmployeeCommand = new DelegateCommand(async () => await ExecuteCommand(UpdateEmployeeDataAsync));
        DeleteEmployeeCommand = new DelegateCommand<List<Employee>>(
            async employees => await ExecuteCommand(() => DeleteEmployeeDataAsync(employees)));

        ResetNewItemCommand.Execute();
    }

    private delegate Task ExecuteCommandCallback();

    public DelegateCommand ResetNewItemCommand { get; }

    public DelegateCommand RefreshCommand { get; }

    public DelegateCommand AddCommand { get; }

    public DelegateCommand UpdateCommand { get; }

    public DelegateCommand<List<Order>> DeleteCommand { get; }

    public DelegateCommand RefreshEmployeesCommand { get; }

    public DelegateCommand AddEmployeeCommand { get; }

    public DelegateCommand UpdateEmployeeCommand { get; }

    public DelegateCommand<List<Employee>> DeleteEmployeeCommand { get; }

    public Order NewItem { get; set; }

    public Order SelectedItem { get; set; } = new ();

    public Employee NewEmployee { get; set; }

    public ObservableCollection<Order> Items => Model.Items;

    public ObservableCollection<Employee> Employees => Model.Employees;

    public bool ActionsBorderIsEnabled { get; set; }

    public bool EmployeesActionsBorderIsEnabled { get; set; }

    private OrdersModel Model { get; }

    private async Task ExecuteCommand(ExecuteCommandCallback method)
    {
        ActionsBorderIsEnabled = false;
        EmployeesActionsBorderIsEnabled = false;
        RaisePropertyChanged(nameof(ActionsBorderIsEnabled));
        RaisePropertyChanged(nameof(EmployeesActionsBorderIsEnabled));
        await method.Invoke();
        string methodName = method.GetMethodInfo().Name;
        if (methodName.Contains("AddData") || methodName.Contains("UpdateData"))
        {
            ResetNewItem();
        }

        ActionsBorderIsEnabled = true;
        RaisePropertyChanged(nameof(ActionsBorderIsEnabled));
        if (SelectedItem.Number == default)
        {
            return;
        }

        if (methodName.Contains("AddEmployeeData") || methodName.Contains("UpdateEmployeeData"))
        {
            ResetNewEmployee();
        }

        EmployeesActionsBorderIsEnabled = true;
        RaisePropertyChanged(nameof(EmployeesActionsBorderIsEnabled));
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

    private async Task UpdateDataAsync()
    {
        var queryResult = await Task.Run(() => Model.GetActualItem(NewItem));
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

        var commandResult = await Task.Run(() => Model.UpdateItemAsync(itemToUpdate));
        if (!commandResult.IsSuccessful)
        {
            MessageBox.Show(commandResult.ErrorMessage, @"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

            return;
        }

        Model.UpdateItemInView(itemToUpdate);
        MessageBox.Show(@"Успешно обновлено", @"Успех", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async Task LoadEmployeesDataAsync()
    {
        if (SelectedItem.Number == default)
        {
            Model.RefreshEmployees(new List<Employee>());

            return;
        }

        var queryResult = await Task.Run(() => Model.GetEmployeesAsync(SelectedItem.Number));
        if (!queryResult.IsSuccessful)
        {
            MessageBox.Show(queryResult.ErrorMessage, @"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

            return;
        }

        var employees = queryResult.Data;
        if (employees.IsNullOrEmpty())
        {
            Model.RefreshEmployees(new List<Employee>());

            return;
        }

        Model.RefreshEmployees(employees);
    }

    private async Task AddEmployeeDataAsync()
    {
        bool anyNewItemUserPropertiesValuesIsNull = typeof(User).GetProperties()
            .Where(property => property.Name is "Login")
            .Any(property => property.GetValue(NewEmployee.User) is null);

        if (anyNewItemUserPropertiesValuesIsNull)
        {
            MessageBox.Show(@"Не все поля заполнены", @"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

            return;
        }

        bool isEmployeeAttachedToOrder = Employees.Any(employee => employee.User.Login == NewEmployee.User.Login);
        if (isEmployeeAttachedToOrder)
        {
            MessageBox.Show(@"Заказ уже прикреплён к этому сотруднику", @"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

            return;
        }

        var queryResult = await Task.Run(() => Model.GetEmployeeByLogin(NewEmployee.User.Login));
        if (!queryResult.IsSuccessful)
        {
            MessageBox.Show(queryResult.ErrorMessage, @"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

            return;
        }

        var employee = queryResult.Data;
        var commandResult = await Task.Run(() => Model.AttachEmployeeAsync(SelectedItem, employee));
        if (!commandResult.IsSuccessful)
        {
            MessageBox.Show(commandResult.ErrorMessage, @"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

            return;
        }

        Model.AddEmployeeToView(employee);
        MessageBox.Show(@"Успешно добавлено", @"Успех", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async Task UpdateEmployeeDataAsync()
    {
        NewEmployee.Id = NewItem.Id;
        var queryResult = await Task.Run(() => Model.GetActualEmployee(NewEmployee));
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

        var commandResult = await Task.Run(() => Model.UpdateEmployeeAsync(itemToUpdate));
        if (!commandResult.IsSuccessful)
        {
            MessageBox.Show(commandResult.ErrorMessage, @"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

            return;
        }

        Model.UpdateEmployeeInView(itemToUpdate);
        MessageBox.Show(@"Успешно обновлено", @"Успех", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async Task DeleteEmployeeDataAsync(List<Employee> items)
    {
        var commandResult = await Task.Run(() => Model.DettachEmployeeAsync(SelectedItem, items));
        if (!commandResult.IsSuccessful)
        {
            MessageBox.Show(commandResult.ErrorMessage, @"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

            return;
        }

        Model.DeleteEmployeeFromView(items);
        MessageBox.Show(@"Успешно удалено", @"Успех", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ResetNewItem()
    {
        NewItem = new Order
        {
            Customer = new Customer
            {
                User = new User(),
            },
            StatementOfWork = new StatementOfWork(),
        };
    }

    private void ResetNewEmployee()
    {
        NewEmployee = new Employee
        {
            User = new User(),
            Passport = new Passport(),
        };
    }
}