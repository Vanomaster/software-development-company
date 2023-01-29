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

public class UsersViewModel : BindableBase, IViewModelBase<User>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LoginViewModel"/> class.
    /// </summary>
    /// <param name="serviceProvider">Service provider.</param>
    public UsersViewModel(IServiceProvider serviceProvider)
    {
        Model = serviceProvider.GetRequiredService<UsersModel>();
        Model.PropertyChanged += (sender, args) => { RaisePropertyChanged(args.PropertyName); };
        SetLoadedItemIdCommand = new DelegateCommand<Guid?>(SetLoadedItemId);
        ResetNewItemCommand = new DelegateCommand(ResetNewItem);
        RefreshCommand = new DelegateCommand(async () => await ExecuteCommand(LoadDataAsync));
        AddCommand = new DelegateCommand(async () => await ExecuteCommand(AddDataAsync));
        UpdateCommand = new DelegateCommand(async () => await ExecuteCommand(UpdateDataAsync));
        DeleteCommand = new DelegateCommand<List<User>>(
            async users => await ExecuteCommand(() => DeleteDataAsync(users)));

        SetPasswordCommand = new DelegateCommand<string>(
            async password => await ExecuteCommand(() => SetPasswordAsync(password)));

        ResetNewItemCommand.Execute();
    }

    private delegate Task ExecuteCommandCallback();

    public DelegateCommand ResetNewItemCommand { get; }

    public DelegateCommand<Guid?> SetLoadedItemIdCommand { get; }

    public DelegateCommand RefreshCommand { get; }

    public DelegateCommand AddCommand { get; }

    public DelegateCommand UpdateCommand { get; }

    public DelegateCommand<List<User>> DeleteCommand { get; }

    public DelegateCommand<string> SetPasswordCommand { get; }

    public User NewItem { get; set; }

    public User SelectedItem { get; set; }

    public ObservableCollection<User> Items => Model.Items;

    public bool ActionsBorderIsEnabled { get; set; }

    public Guid? LoadedItemId { get; set; }

    private UsersModel Model { get; }

    private async Task ExecuteCommand(ExecuteCommandCallback method)
    {
        ActionsBorderIsEnabled = false;
        RaisePropertyChanged(nameof(ActionsBorderIsEnabled));
        await method.Invoke();
        string methodName = method.GetMethodInfo().Name;
        if (methodName.Contains("AddData") || methodName.Contains("UpdateData"))
        {
            ResetNewItem();
        }

        ActionsBorderIsEnabled = true;
        RaisePropertyChanged(nameof(ActionsBorderIsEnabled));
    }

    private void SetLoadedItemId(Guid? id)
    {
        LoadedItemId = id;
        ResetNewItem();
    }

    private async Task LoadDataAsync()
    {
        var queryResult = await Task.Run(() => Model.GetItemsAsync(LoadedItemId));
        if (!queryResult.IsSuccessful)
        {
            MessageBox.Show(queryResult.ErrorMessage, @"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

            return;
        }

        var items = queryResult.Data;
        if (LoadedItemId is not null
            && items.FirstOrDefault().Customer is null
            && items.FirstOrDefault().Employee is null)
        {
            MessageBox.Show(
                @"Не заполнены данные о пользователе. Обратитесь к администратору",
                @"Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            return;
        }

        Model.RefreshItems(items);
    }

    private async Task AddDataAsync()
    {
        bool anyNewItemPropertiesValuesIsNull = typeof(User).GetProperties()
            .Where(property => property.Name is not "Id" and not "Customer" and not "Employee")
            .Any(property => property.GetValue(NewItem) is null);

        if (anyNewItemPropertiesValuesIsNull || NewItem.Role == 0)
        {
            MessageBox.Show(@"Не все поля заполнены", @"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

            return;
        }

        var validateCommandResult = Model.ValidateUser(NewItem, LoadedItemId);
        if (!validateCommandResult.IsSuccessful)
        {
            MessageBox.Show(validateCommandResult.ErrorMessage, @"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

            return;
        }

        NewItem.Login = NewItem.Login.Trim();
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
        var queryResult = Model.GetActualItem(NewItem, LoadedItemId);
        if (!queryResult.IsSuccessful)
        {
            MessageBox.Show(queryResult.ErrorMessage, @"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

            return;
        }

        var userToUpdate = queryResult.Data;
        var validateCommandResult = Model.ValidateUser(userToUpdate, LoadedItemId);
        if (!validateCommandResult.IsSuccessful)
        {
            MessageBox.Show(validateCommandResult.ErrorMessage, @"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

            return;
        }

        var commandResult = await Task.Run(() => Model.UpdateItemAsync(userToUpdate));
        if (!commandResult.IsSuccessful)
        {
            MessageBox.Show(commandResult.ErrorMessage, @"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

            return;
        }

        Model.UpdateItemInView(userToUpdate);
        MessageBox.Show(@"Успешно обновлено", @"Успех", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async Task DeleteDataAsync(List<User> users)
    {
        var commandResult = await Task.Run(() => Model.DeleteItemAsync(users));
        if (!commandResult.IsSuccessful)
        {
            MessageBox.Show(commandResult.ErrorMessage, @"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

            return;
        }

        Model.DeleteItemFromView(users);
        MessageBox.Show(@"Успешно удалено", @"Успех", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async Task SetPasswordAsync(string password)
    {
        ActionsBorderIsEnabled = false;
        RaisePropertyChanged(nameof(ActionsBorderIsEnabled));
        var commandResult = await Task.Run(() => Model.SetUserPasswordProperties(password, NewItem));
        if (!commandResult.IsSuccessful)
        {
            MessageBox.Show(commandResult.ErrorMessage, @"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            ResetNewItem();
            ActionsBorderIsEnabled = true;
            RaisePropertyChanged(nameof(ActionsBorderIsEnabled));

            return;
        }

        ActionsBorderIsEnabled = true;
        RaisePropertyChanged(nameof(ActionsBorderIsEnabled));
    }

    private void ResetNewItem()
    {
        if (LoadedItemId == null)
        {
            NewItem = new User();
        }

        if (LoadedItemId != null)
        {
            NewItem = new User
            {
                Customer = new Customer(),
                Employee = new Employee(),
            };
        }
    }
}