using System;
using System.Threading.Tasks;
using System.Windows;
using Gui.Models;
using Microsoft.Extensions.DependencyInjection;
using Prism.Commands;
using Prism.Mvvm;

namespace Gui.ViewModels;

/// <summary>
/// LoginWindow ViewModel.
/// </summary>
public class LoginViewModel : BindableBase
{
    private string login = null!;
    private string password = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginViewModel"/> class.
    /// </summary>
    /// <param name="serviceProvider">Service provider.</param>
    public LoginViewModel(IServiceProvider serviceProvider)
    {
        Model = serviceProvider.GetRequiredService<LoginModel>();
        Model.PropertyChanged += (sender, args) => { RaisePropertyChanged(args.PropertyName); };
        LoginCommand = new DelegateCommand(async () => await LoginAsync());
    }

    /// <summary>
    /// Login command.
    /// </summary>
    public DelegateCommand LoginCommand { get; }

    public bool BtnLoginIsEnabled { get; set; } = true;

    /// <summary>
    /// Login.
    /// </summary>
    public string Login
    {
        get => login;

        set
        {
            login = value;
            RaisePropertyChanged();
        }
    }

    /// <summary>
    /// Login.
    /// </summary>
    public string Password
    {
        get => password;

        set
        {
            password = value; // TODO get pass hash
            RaisePropertyChanged();
        }
    }

    private LoginModel Model { get; }

    private async Task LoginAsync()
    {
        BtnLoginIsEnabled = false;
        RaisePropertyChanged(nameof(BtnLoginIsEnabled));
        var authorizationResult = await Task.Run(() => Model.Login(login, password));
        if (!authorizationResult.IsSuccessful)
        {
            MessageBox.Show(authorizationResult.ErrorMessage, @"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            BtnLoginIsEnabled = true;
            RaisePropertyChanged(nameof(BtnLoginIsEnabled));

            return;
        }

        var userPrincipal = authorizationResult.Data;
        Model.RunMainWindow(userPrincipal);
        Model.CloseLoginWindow();

        // Model.SaveToFileUserData(userPrincipal);
    }
}