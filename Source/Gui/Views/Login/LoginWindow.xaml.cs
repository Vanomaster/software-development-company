using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Gui.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Gui.Views;

/// <summary>
/// Interaction logic for LoginWindow.xaml.
/// </summary>
public partial class LoginWindow : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LoginWindow"/> class.
    /// </summary>
    /// <param name="serviceProvider">Service provider.</param>
    public LoginWindow(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        ViewModel = serviceProvider.GetRequiredService<LoginViewModel>();
        DataContext = ViewModel;
    }

    private LoginViewModel ViewModel { get; }

    private void TopRowMouseDown(object sender, MouseButtonEventArgs args)
    {
        CommonActions.TopRowMouseDown(this, args);
    }

    private void BtnHideClick(object sender, RoutedEventArgs args)
    {
        CommonActions.BtnHideClick(this);
    }

    private void BtnExitClick(object sender, RoutedEventArgs args)
    {
        CommonActions.BtnExitClick();
    }

    private void PasswordChanged(object sender, RoutedEventArgs args)
    {
        if (sender is not PasswordBox passBox)
        {
            return;
        }

        PasswordBoxAttachedProperties.SetEncryptedPassword(passBox, passBox.Password);
    }
}