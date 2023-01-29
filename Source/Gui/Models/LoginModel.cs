using System;
using System.Threading.Tasks;
using System.Windows;
using AuthorizationService;
using CleanModels;
using Core;
using Gui.Views;
using Gui.Views.Main.Pages;
using Microsoft.Extensions.DependencyInjection;
using Prism.Mvvm;
using Queries.Base;

namespace Gui.Models;

/// <summary>
/// Login model.
/// </summary>
public class LoginModel : BindableBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LoginModel"/> class.
    /// </summary>
    /// <param name="serviceProvider">Service provider.</param>
    public LoginModel(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    /// <summary>
    /// Service provider.
    /// </summary>
    private static IServiceProvider ServiceProvider { get; set; } = null!;

    /// <summary>
    /// Login.
    /// </summary>
    /// <param name="username">Username.</param>
    /// <param name="password">Password.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<QueryResult<UserPrincipal>> Login(string username, string password)
    {
        var authorizationModel = new AuthorizationModel
        {
            Username = username,
            Password = password,
        };

        var authorizer = ServiceProvider.GetRequiredService<Authorizer>();
        var authorizationResult = await authorizer.Authorize(authorizationModel);
        if (!authorizationResult.IsSuccessful)
        {
            return new QueryResult<UserPrincipal>(authorizationResult.ErrorMessage);
        }

        return new QueryResult<UserPrincipal>(authorizationResult.Data);
        /*loginWindow.Dispatcher.Invoke(() => RunMainWindow(authorizationResult.Data.Role));
        loginWindow.Dispatcher.Invoke(CloseLoginWindow);*/
    }

    public void SaveToFileUserData(UserPrincipal userData)
    {
        var fileConverter = ServiceProvider.GetRequiredService<FileConverter>();
        fileConverter.WriteToJsonFile(Constants.UserDataPath, userData);
    }

    public void RunMainWindow(UserPrincipal userPrincipal)
    {
        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        InitMainWindowByUserRole(mainWindow, userPrincipal);
        mainWindow.Show();
    }

    public void CloseLoginWindow()
    {
        foreach (object? windowObject in Application.Current.Windows)
        {
            if (windowObject is not LoginWindow window)
            {
                continue;
            }

            window.Close();
        }
    }

    private static void InitMainWindowByUserRole(MainWindow mainWindow, UserPrincipal userPrincipal)
    {
        switch (userPrincipal.Role)
        {
            case UserRole.Customer:
            {
                InitPagesForCustomer(mainWindow, userPrincipal.TypedIdentity.Id);
                break;
            }

            case UserRole.Employee:
            {
                InitPagesForEmployee(mainWindow, userPrincipal.TypedIdentity.Id);
                break;
            }

            case UserRole.Administrator:
            {
                InitPagesForAdmin(mainWindow);
                break;
            }

            default:
            {
                MessageBox.Show(@"Ошибка при определении прав");

                break;
            }
        }
    }

    private static void InitPagesForCustomer(MainWindow mainWindow, Guid id)
    {
        mainWindow.BtnEmployees.Visibility = Visibility.Collapsed;
        mainWindow.BtnOrders.Visibility = Visibility.Collapsed;
        mainWindow.BtnCustomersTextBlock.Text = @"Заказы";

        var customersPage = ServiceProvider.GetRequiredService<CustomersPage>();
        customersPage.TitleTextBlock.Text = @"Заказы";
        customersPage.ActionsBorder.Visibility = Visibility.Collapsed;
        customersPage.OrdersTitleBorder.Visibility = Visibility.Collapsed;
        customersPage.BtnDeleteOrders.Visibility = Visibility.Collapsed;
        customersPage.TextBoxFilterOrders.Width = 1190;
        customersPage.DgCustomers.Visibility = Visibility.Collapsed;
        customersPage.SetLoadedItemId(id);
        mainWindow.ViewModel.SetCustomersPageCommand.Execute(customersPage);

        var ordersPage = ServiceProvider.GetRequiredService<OrdersPage>();
        mainWindow.ViewModel.SetOrdersPageCommand.Execute(ordersPage);

        var employeesPage = ServiceProvider.GetRequiredService<EmployeesPage>();
        mainWindow.ViewModel.SetEmployeesPageCommand.Execute(employeesPage);

        var usersPage = GetUsersPageForNotAdmin(mainWindow, id);
        const int employeeColumnStartIndex = 7;
        const int employeeColumnCount = 10;
        for (int i = employeeColumnStartIndex; i < employeeColumnStartIndex + employeeColumnCount; i++)
        {
            usersPage.DgUsers.Columns[i].Visibility = Visibility.Collapsed;
        }

        mainWindow.ViewModel.SetUsersPageCommand.Execute(usersPage);
    }

    private static void InitPagesForEmployee(MainWindow mainWindow, Guid id)
    {
        mainWindow.BtnCustomers.Visibility = Visibility.Collapsed;

        var customersPage = ServiceProvider.GetRequiredService<CustomersPage>();
        mainWindow.ViewModel.SetCustomersPageCommand.Execute(customersPage);

        var ordersPage = ServiceProvider.GetRequiredService<OrdersPage>();
        foreach (var column in ordersPage.DgEmployees.Columns)
        {
            column.IsReadOnly = true;
        }

        /*ordersPage.DgEmployees.Columns[1].Visibility = Visibility.Collapsed;
        ordersPage.DgEmployees.Columns[2].Visibility = Visibility.Collapsed;
        ordersPage.DgEmployees.Columns[8].Visibility = Visibility.Collapsed;*/
        mainWindow.ViewModel.SetOrdersPageCommand.Execute(ordersPage);

        var employeesPage = ServiceProvider.GetRequiredService<EmployeesPage>();
        employeesPage.BtnAdd.Visibility = Visibility.Collapsed;
        employeesPage.BtnDelete.Visibility = Visibility.Collapsed;
        employeesPage.TextBoxFilter.Width = 1320;
        foreach (var column in employeesPage.DgEmployees.Columns)
        {
            column.IsReadOnly = true;
        }

        employeesPage.DgEmployees.Columns[1].Visibility = Visibility.Collapsed;
        employeesPage.DgEmployees.Columns[2].Visibility = Visibility.Collapsed;
        employeesPage.DgEmployees.Columns[8].Visibility = Visibility.Collapsed;
        //employeesPage.SetLoadedItemId(id); // TODO
        mainWindow.ViewModel.SetEmployeesPageCommand.Execute(employeesPage);

        var usersPage = GetUsersPageForNotAdmin(mainWindow, id);
        const int customerColumnStartIndex = 2;
        const int customerColumnCount = 5;
        for (int i = customerColumnStartIndex; i < customerColumnStartIndex + customerColumnCount; i++)
        {
            usersPage.DgUsers.Columns[i].Visibility = Visibility.Collapsed;
        }

        mainWindow.ViewModel.SetUsersPageCommand.Execute(usersPage);
    }

    private static void InitPagesForAdmin(MainWindow mainWindow)
    {
        var customersPage = ServiceProvider.GetRequiredService<CustomersPage>();
        mainWindow.ViewModel.SetCustomersPageCommand.Execute(customersPage);

        var ordersPage = ServiceProvider.GetRequiredService<OrdersPage>();
        mainWindow.ViewModel.SetOrdersPageCommand.Execute(ordersPage);

        var employeesPage = ServiceProvider.GetRequiredService<EmployeesPage>();
        mainWindow.ViewModel.SetEmployeesPageCommand.Execute(employeesPage);

        var usersPage = ServiceProvider.GetRequiredService<UsersPage>();
        foreach (var column in usersPage.DgUsers.Columns)
        {
            column.Visibility = Visibility.Collapsed;
        }

        usersPage.DgUsers.Columns[0].Visibility = Visibility.Visible;
        usersPage.DgUsers.Columns[1].Visibility = Visibility.Visible;
        usersPage.DgUsers.Columns[^3].Visibility = Visibility.Visible;
        usersPage.DgUsers.Columns[^2].Visibility = Visibility.Visible;
        usersPage.DgUsers.Columns[^1].Visibility = Visibility.Visible;
        mainWindow.ViewModel.SetUsersPageCommand.Execute(usersPage);
    }

    private static UsersPage GetUsersPageForNotAdmin(MainWindow mainWindow, Guid id)
    {
        mainWindow.BtnUsersTextBlock.Text = @"Профиль";
        var usersPage = ServiceProvider.GetRequiredService<UsersPage>();
        usersPage.TitleTextBlock.Text = @"Профиль";
        usersPage.ActionsBorder.Visibility = Visibility.Collapsed;
        usersPage.DgUsers.Columns[^2].Visibility = Visibility.Collapsed;
        usersPage.DgUsers.Columns[^1].Visibility = Visibility.Collapsed;
        usersPage.SetLoadedItemId(id);

        return usersPage;
    }
}