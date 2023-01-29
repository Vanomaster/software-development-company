using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dal.Entities;
using Gui.ViewModels;
using MaterialDesignExtensions.Controls;
using Prism.Commands;

namespace Gui.Views;

public class CommonActions
{
    public static void TopRowMouseDown(Window window, MouseButtonEventArgs args)
    {
        if (args.ChangedButton == MouseButton.Left)
        {
            window.DragMove();
        }
    }

    public static void BtnHideClick(Window window)
    {
        window.WindowState = WindowState.Minimized;
    }

    public static void BtnExitClick()
    {
        Application.Current.Shutdown();
    }

    /*public static async Task LoadData<TEntity>(
        DelegateCommand refreshCommand,
        IViewModelBase<TEntity> viewModel,
        PersistentSearch txtBoxFilter,
        Button btnDelete,
        Button btnAdd)
        where TEntity : IEntity
    {
        txtBoxFilter.IsEnabled = false;
        btnDelete.IsEnabled = false;
        btnAdd.IsEnabled = false;
        refreshCommand.Execute();
        /*await Task.Run(() => WaitingForCommandResult(viewModel));
        if (viewModel.IsSuccessfulCommandResult.HasValue && !viewModel.IsSuccessfulCommandResult.Value)
        {
            viewModel.ResetCommandResultCommand.Execute();

            return;
        }

        viewModel.ResetCommandResultCommand.Execute();#1#
        txtBoxFilter.IsEnabled = true;
        btnDelete.IsEnabled = true;
        btnAdd.IsEnabled = true;
    }*/

    public static void BeginningEditHandler<TEntity>(
        DataGrid dataGrid,
        DataGridBeginningEditEventArgs args,
        IViewModelBase<TEntity> viewModel)
        where TEntity : IEntity
    {
        if (dataGrid.CanUserAddRows && !args.Row.IsNewItem)
        {
            args.Cancel = true;

            return;
        }

        if (dataGrid.CanUserAddRows && args.Row.IsNewItem)
        {
            return;
        }

        if (args.Row.Item is not TEntity editableItem)
        {
            return;
        }

        if (editableItem.Id == viewModel.NewItem.Id)
        {
            return;
        }

        viewModel.NewItem.Id = editableItem.Id;
    }

    public static void BeginningEditAdditionalHandler<TEntity, TEditableEntity>(
        DataGrid dataGrid,
        DataGridBeginningEditEventArgs args,
        IViewModelBase<TEntity> viewModel)
        where TEntity : IEntity
        where TEditableEntity : IEntity
    {
        if (dataGrid.CanUserAddRows && !args.Row.IsNewItem)
        {
            args.Cancel = true;

            return;
        }

        if (dataGrid.CanUserAddRows && args.Row.IsNewItem)
        {
            return;
        }

        if (args.Row.Item is not TEditableEntity editableItem)
        {
            return;
        }

        if (editableItem.Id == viewModel.NewItem.Id)
        {
            return;
        }

        viewModel.NewItem.Id = editableItem.Id;
    }

    public static async void RowEditEndingHandler<TEntity>(
        DataGrid dataGrid,
        DataGridRowEditEndingEventArgs args,
        IViewModelBase<TEntity> viewModel,
        DelegateCommand updateCommand,
        DelegateCommand addCommand)
        where TEntity : IEntity
    {
        IEditableCollectionView editableCollectionView = dataGrid.Items;
        if (!args.Row.IsNewItem && editableCollectionView.IsEditingItem)
        {
            if (!ConfirmNeedTo("Желаете ли вы сохранить изменения?"))
            {
                if (editableCollectionView.CanCancelEdit)
                {
                    editableCollectionView.CancelEdit();
                }

                viewModel.ResetNewItemCommand.Execute();
                /*await Task.Run(() => WaitingForCommandResult(viewModel));
                viewModel.ResetCommandResultCommand.Execute();*/

                return;
            }

            if (editableCollectionView.CanCancelEdit)
            {
                editableCollectionView.CancelEdit();
            }

            editableCollectionView.CommitEdit();
            updateCommand.Execute();
            /*await Task.Run(() => WaitingForCommandResult(viewModel));
            viewModel.ResetCommandResultCommand.Execute();*/
        }

        if (args.Row.IsNewItem && !editableCollectionView.IsEditingItem)
        {
            if (!ConfirmNeedTo("Желаете ли вы сохранить изменения?"))
            {
                editableCollectionView.CancelNew();

                return;
            }

            editableCollectionView.CancelNew();
            addCommand.Execute();
            /*await Task.Run(() => WaitingForCommandResult(viewModel));
            if (viewModel.IsSuccessfulCommandResult.HasValue && !viewModel.IsSuccessfulCommandResult!.Value)
            {
                args.Cancel = true;

                return;
            }

            viewModel.ResetCommandResultCommand.Execute();*/
        }
    }

    public static async void DeleteData<TEntity, TDeletableEntity>(
        DataGrid dataGrid,
        DelegateCommand<List<TDeletableEntity>> deleteCommand,
        IViewModelBase<TEntity> viewModel)
        where TEntity : IEntity
        where TDeletableEntity : IEntity
    {
        if (dataGrid.SelectedItem is not TDeletableEntity)
        {
            return;
        }

        if (!ConfirmNeedTo("Желаете ли вы удалить запись?"))
        {
            return;
        }

        var selectedUsers = dataGrid.SelectedItems
            .Cast<TDeletableEntity>()
            .ToList();

        deleteCommand.Execute(selectedUsers);
        /*await Task.Run(() => WaitingForCommandResult(viewModel));
        viewModel.ResetCommandResultCommand.Execute();*/
    }

    public static void ChangeViewToAdd(DataGrid dataGrid, TextBlock btnAddTextBlock)
    {
        if (dataGrid.CanUserAddRows)
        {
            ChangeCommonViewToAdd(dataGrid, btnAddTextBlock, isDirectConversion: false);

            return;
        }

        ChangeCommonViewToAdd(dataGrid, btnAddTextBlock, isDirectConversion: true);
    }

    public static void ChangeCommonViewToAdd(DataGrid dataGrid, TextBlock btnAddTextBlock, bool isDirectConversion)
    {
        IEditableCollectionView editableCollectionView = dataGrid.Items;
        if (!isDirectConversion)
        {
            editableCollectionView.CancelNew();
        }

        dataGrid.CanUserAddRows = isDirectConversion;
        if (isDirectConversion)
        {
            editableCollectionView.NewItemPlaceholderPosition = NewItemPlaceholderPosition.AtBeginning;
        }

        btnAddTextBlock.Text = isDirectConversion ? @"Отменить" : @"Добавить";
        btnAddTextBlock.ToolTip = isDirectConversion ? @"Выйти из режима добавления элементов" : @"Перейти в режим добавления элементов";
    }

    public static void ChangeColumnVisibility(
        DataGrid dataGrid,
        string currentColumnHeaderContent,
        Visibility newColumnVisibility)
    {
        var columnQueryResult = dataGrid.GetColumnByHeaderContent(currentColumnHeaderContent);
        if (!columnQueryResult.IsSuccessful)
        {
            MessageBox.Show(columnQueryResult.ErrorMessage);

            return;
        }

        columnQueryResult.Data.Visibility = newColumnVisibility;
    }

    public static async void SelectionChangedHandler<TEntity>(
        DataGrid dataGrid,
        DelegateCommand refreshCommand,
        IViewModelBase<TEntity> viewModel)
        where TEntity : IEntity
    {
        if (dataGrid.SelectedItem is not TEntity item)
        {
            return;
        }

        viewModel.SelectedItem = item;
        refreshCommand.Execute();
        // await LoadData(refreshCommand, viewModel, txtBoxFilter, btnDelete, btnAdd);
    }

    public static bool ConfirmNeedTo(string messageBoxText)
    {
        var dialogResult = MessageBox.Show(
            messageBoxText,
            "Предупреждение",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        bool isNeedToSave = dialogResult switch
        {
            MessageBoxResult.Yes => true,
            MessageBoxResult.No => false,
            MessageBoxResult.None => false,
            _ => throw new ArgumentOutOfRangeException("There is no such MessageBoxResult"),
        };

        return isNeedToSave;
    }

    /*public static void WaitingForCommandResult<TEntity>(IViewModelBase<TEntity> viewModel)
    where TEntity : IEntity
    {
        while (!viewModel.IsSuccessfulCommandResult.HasValue)
        {
        }
    }*/

    private static Window GetWindow(object sender)
    {
        Window window = null!;
        if (sender is not FrameworkElement frameworkElement)
        {
            throw new Exception("sender is not FrameworkElement");
        }

        while (window == null)
        {
            if (frameworkElement.Parent is Window parentWindow)
            {
                window = parentWindow;
            }

            if (frameworkElement.Parent is not FrameworkElement frameworkElementParent)
            {
                throw new Exception("frameworkElement.Parent is not FrameworkElement");
            }

            frameworkElement = frameworkElementParent;
        }

        return window;
    }
}