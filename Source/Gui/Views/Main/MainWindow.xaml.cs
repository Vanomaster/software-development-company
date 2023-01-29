using System;
using System.Windows;
using System.Windows.Input;
using Gui.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Gui.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml.
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// </summary>
    /// <param name="serviceProvider">Service provider.</param>
    public MainWindow(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        ViewModel = serviceProvider.GetRequiredService<MainViewModel>();
        DataContext = ViewModel;
    }

    public MainViewModel ViewModel { get; }

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
}