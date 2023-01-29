using System;
using System.Windows;
using System.Windows.Controls;
using Gui.Views;
using Gui.Views.Main.Pages;
using Microsoft.Extensions.DependencyInjection;
using Prism.Mvvm;

namespace Gui.Models;

/// <summary>
/// Main model.
/// </summary>
public class MainModel : BindableBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LoginModel"/> class.
    /// </summary>
    /// <param name="serviceProvider">Service provider.</param>
    public MainModel(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    /// <summary>
    /// Service provider.
    /// </summary>
    private static IServiceProvider ServiceProvider { get; set; } = null!;

    private static CustomersPage CustomersPage { get; set; } = null!;

    private static EmployeesPage EmployeesPage { get; set; } = null!;

    private static OrdersPage OrdersPage { get; set; } = null!;

    private static UsersPage UsersPage { get; set; } = null!;

    public void SetCustomersPage(CustomersPage page)
    {
        CustomersPage = page;
    }

    public void SetEmployeesPage(EmployeesPage page)
    {
        EmployeesPage = page;
    }

    public void SetOrdersPage(OrdersPage page)
    {
        OrdersPage = page;
    }

    public void SetUsersPage(UsersPage page)
    {
        UsersPage = page;
    }

    public void OpenCustomersPage()
    {
        OpenPage(CustomersPage);
    }

    public void OpenOrdersPage()
    {
        OpenPage(OrdersPage);
    }

    public void OpenEmployeesPage()
    {
        OpenPage(EmployeesPage);
    }

    public void OpenUsersPage()
    {
        OpenPage(UsersPage);
    }

    public void LogOut()
    {
        OpenWindow<LoginWindow>();
        CloseWindow<MainWindow>();
    }

    private static void OpenPage(Page page)
    {
        foreach (object? windowObject in Application.Current.Windows)
        {
            if (windowObject is not MainWindow window)
            {
                continue;
            }

            window.Page.Content = page;
        }
    }

    private static void OpenWindow<TWindow>()
        where TWindow : Window
    {
        var window = ServiceProvider.GetRequiredService<TWindow>();
        window.Show();
    }

    private static void CloseWindow<TWindow>()
        where TWindow : Window
    {
        foreach (object? windowObject in Application.Current.Windows)
        {
            if (windowObject is not TWindow window)
            {
                continue;
            }

            window.Close();
        }
    }
}