using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using AuthorizationService;
using Commands;
using Commands.Base;
using Commands.Database;
using Core;
using Dal;
using Gui.Views;
using Microsoft.Extensions.DependencyInjection;
using Queries;

namespace Gui;

/// <summary>
/// Interaction logic for App.xaml.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Service provider.
    /// </summary>
    private IServiceProvider ServiceProvider { get; set; } = null!;

    /// <inheritdoc/>
    protected override void OnStartup(StartupEventArgs e)
    {
        ConfigureExceptionsHandling();
        ConfigureServices();
        TestDbConnection();
        RunLoginWindow();
    }

    private void ConfigureExceptionsHandling()
    {
        AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionThrowing;
        TaskScheduler.UnobservedTaskException += UnobservedTaskExceptionThrowing;
    }

    private void ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddGui();
        services.AddDal();
        services.AddCore();
        services.AddQueries();
        services.AddCommands();
        services.AddAuthenticationService();
        ServiceProvider = services.BuildServiceProvider();
    }

    private async void TestDbConnection()
    {
        var command = ServiceProvider.GetRequiredService<TestConnectionCommand>();
        var commandResult = await command.ExecuteAsync();
        if (!commandResult.IsSuccessful)
        {
            MessageBox.Show(commandResult.ErrorMessage, @"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void RunLoginWindow()
    {
        Current.ShutdownMode = ShutdownMode.OnLastWindowClose;
        var loginWindow = ServiceProvider.GetRequiredService<LoginWindow>();
        loginWindow.Show();
    }

    private void UnhandledExceptionThrowing(object sender, UnhandledExceptionEventArgs e)
    {
        var errorMessage = @$"Произошло необработанное исключение: {e.ExceptionObject}";
        MessageBox.Show(errorMessage, @"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

        // TODO <configuration><runtime><legacyUnhandledExceptionPolicy enabled="1" /></runtime></configuration> in config file
    }

    private void UnobservedTaskExceptionThrowing(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        var errorMessage = @$"Произошло необработанное исключение: {e.Exception}";
        MessageBox.Show(errorMessage, @"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        e.SetObserved();
    }
}