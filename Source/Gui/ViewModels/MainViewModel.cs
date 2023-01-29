using System;
using Gui.Models;
using Gui.Views;
using Gui.Views.Main.Pages;
using Microsoft.Extensions.DependencyInjection;
using Prism.Commands;
using Prism.Mvvm;

namespace Gui.ViewModels;

public class MainViewModel : BindableBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LoginViewModel"/> class.
    /// </summary>
    /// <param name="serviceProvider">Service provider.</param>
    public MainViewModel(IServiceProvider serviceProvider)
    {
        Model = serviceProvider.GetRequiredService<MainModel>();
        Model.PropertyChanged += (sender, args) => { RaisePropertyChanged(args.PropertyName); };
        SetCustomersPageCommand = new DelegateCommand<CustomersPage>(Model.SetCustomersPage);
        SetEmployeesPageCommand = new DelegateCommand<EmployeesPage>(Model.SetEmployeesPage);
        SetOrdersPageCommand = new DelegateCommand<OrdersPage>(Model.SetOrdersPage);
        SetUsersPageCommand = new DelegateCommand<UsersPage>(Model.SetUsersPage);
        OpenCustomersPageCommand = new DelegateCommand(Model.OpenCustomersPage);
        OpenOrdersPageCommand = new DelegateCommand(Model.OpenOrdersPage);
        OpenEmployeesPageCommand = new DelegateCommand(Model.OpenEmployeesPage);
        OpenUsersPageCommand = new DelegateCommand(Model.OpenUsersPage);
        LogOutCommand = new DelegateCommand(Model.LogOut);
    }

    public DelegateCommand<CustomersPage> SetCustomersPageCommand { get; }

    public DelegateCommand<EmployeesPage> SetEmployeesPageCommand { get; }

    public DelegateCommand<OrdersPage> SetOrdersPageCommand { get; }

    public DelegateCommand<UsersPage> SetUsersPageCommand { get; }

    public DelegateCommand OpenCustomersPageCommand { get; }

    public DelegateCommand OpenOrdersPageCommand { get; }

    public DelegateCommand OpenEmployeesPageCommand { get; }

    public DelegateCommand OpenUsersPageCommand { get; }

    public DelegateCommand LogOutCommand { get; }

    private MainModel Model { get; }
}