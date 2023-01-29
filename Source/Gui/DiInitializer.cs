using Gui.Models;
using Gui.ViewModels;
using Gui.Views;
using Gui.Views.Main.Pages;
using Microsoft.Extensions.DependencyInjection;

namespace Gui;

/// <summary>
/// Registrar of services in services provider.
/// </summary>
public static class DiInitializer
{
    /// <summary>
    /// Register services in services provider.
    /// </summary>
    /// <param name="services">Collection of services.</param>
    /// <returns>Changed collection of services.</returns>
    public static IServiceCollection AddGui(this IServiceCollection services)
    {
        services.AddScoped<DataValidator>();
        services.AddTransient<LoginWindow>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<LoginModel>();
        services.AddTransient<MainWindow>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<MainModel>();
        services.AddTransient<CustomersPage>();
        services.AddTransient<CustomersViewModel>();
        services.AddTransient<CustomersModel>();
        services.AddTransient<EmployeesPage>();
        services.AddTransient<EmployeesViewModel>();
        services.AddTransient<EmployeesModel>();
        services.AddTransient<OrdersPage>();
        services.AddTransient<OrdersViewModel>();
        services.AddTransient<OrdersModel>();
        services.AddTransient<UsersPage>();
        services.AddTransient<UsersViewModel>();
        services.AddTransient<UsersModel>();

        return services;
    }
}