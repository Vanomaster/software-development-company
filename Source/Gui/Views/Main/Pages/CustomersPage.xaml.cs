using System;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using CleanModels;
using Dal.Entities;
using Gui.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Customer = Dal.Entities.Customer;

namespace Gui.Views;

/// <summary>
/// Interaction logic for CustomersPage.xaml.
/// </summary>
public partial class CustomersPage : Page
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CustomersPage"/> class.
    /// </summary>
    /// <param name="serviceProvider">Service provider.</param>
    public CustomersPage(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        ViewModel = serviceProvider.GetRequiredService<CustomersViewModel>();
        DataContext = ViewModel;
        Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);
    }

    private CustomersViewModel ViewModel { get; }

    public void SetLoadedItemId(Guid? itemId)
    {
        ViewModel.SetLoadedItemIdCommand.Execute(itemId);
    }

    private void BeginningEdit(object? sender, DataGridBeginningEditEventArgs args)
    {
        CommonActions.BeginningEditHandler(DgCustomers, args, ViewModel);
    }

    private void CellEditEnding(object? sender, DataGridCellEditEndingEventArgs args)
    {
        if (args.EditingElement is ComboBox comboBox)
        {
            SetCmbBoxValue(args, comboBox);

            return;
        }

        if (args.EditingElement is TextBox textBox)
        {
            SetTextBoxValue(args, textBox);
        }
    }

    private void RowEditEnding(object sender, DataGridRowEditEndingEventArgs args)
    {
        CommonActions.RowEditEndingHandler(DgCustomers, args, ViewModel, ViewModel.UpdateCommand, ViewModel.AddCommand);
    }

    private void BtnDeleteClick(object sender, RoutedEventArgs args)
    {
        CommonActions.DeleteData(DgCustomers, ViewModel.DeleteCommand, ViewModel);
    }

    private void BtnAddClick(object sender, RoutedEventArgs args)
    {
        if (DgCustomers.CanUserAddRows)
        {
            ChangeViewToAdd(isDirectConversion: false);

            return;
        }

        ChangeViewToAdd(isDirectConversion: true);
    }

    private void TextBoxFilterPreviewKeyUp(object sender, KeyEventArgs args)
    {
        string text = TextBoxFilter?.SearchTerm?.ToLower() ?? string.Empty;
        bool Predicate(PropertyInfo property) => property.Name is not "Id" and not "Orders";
        bool AdditionalPredicate(PropertyInfo property) => property.Name is "Login";
        foreach (var item in ViewModel.Items)
        {
            DataGridFilter.FilterWithEnumAndAdditional(item, item.User, DgCustomers, Predicate, AdditionalPredicate, text);
        }
    }

    private void DgCustomersSelectionChanged(object sender, RoutedEventArgs args)
    {
        CommonActions.SelectionChangedHandler(
            DgCustomers,
            ViewModel.RefreshOrdersCommand,
            ViewModel);
    }

    private void OrderBeginningEdit(object? sender, DataGridBeginningEditEventArgs args)
    {
        CommonActions.BeginningEditAdditionalHandler<Customer, Order>(DgOrders, args, ViewModel);
    }

    private void OrderCellEditEnding(object? sender, DataGridCellEditEndingEventArgs args)
    {
        if (args.EditingElement is ComboBox comboBox)
        {
            SetOrderCmbBoxValue(args, comboBox);

            return;
        }

        if (args.EditingElement is TextBox textBox)
        {
            SetOrderTextBoxValue(args, textBox);
        }
    }

    private void OrderRowEditEnding(object? sender, DataGridRowEditEndingEventArgs args)
    {
        CommonActions.RowEditEndingHandler(DgOrders, args, ViewModel, ViewModel.UpdateOrderCommand, ViewModel.AddOrderCommand);
    }

    private void BtnDeleteOrdersClick(object sender, RoutedEventArgs args)
    {
        CommonActions.DeleteData(DgOrders, ViewModel.DeleteOrderCommand, ViewModel);
    }

    private void BtnAddOrdersClick(object sender, RoutedEventArgs args)
    {
        if (DgOrders.CanUserAddRows)
        {
            ChangeViewToAddOrder(isDirectConversion: false);

            return;
        }

        ChangeViewToAddOrder(isDirectConversion: true);
    }

    private void TextBoxFilterOrdersPreviewKeyUp(object sender, KeyEventArgs args)
    {
        string text = TextBoxFilterOrders?.SearchTerm?.ToLower() ?? string.Empty;
        bool Predicate(PropertyInfo property) => property.Name is not "Id" and not "CustomerId" and not "Customer"
            and not "Employees" and not "StatementOfWork";
        bool AdditionalPredicate(PropertyInfo property) => property.Name is not "Id" and not "OrderId" and not "Order";
        bool UserPredicate(PropertyInfo property) => property.Name is "Login";
        foreach (var item in ViewModel.Orders)
        {
            DataGridFilter.FilterWithEnumAndAdditional(item, item.StatementOfWork, item.Customer.User, DgOrders, Predicate, AdditionalPredicate, UserPredicate, text);
        }
    }

    private void SetCmbBoxValue(DataGridCellEditEndingEventArgs args, ComboBox comboBox)
    {
        var cmbBoxColumnHeader = args.Column.Header.ToString();
        if (cmbBoxColumnHeader is null)
        {
            return;
        }

        if (comboBox.SelectionBoxItem is not EnumerationMember enumerationMember)
        {
            return;
        }

        if (enumerationMember.Value is not Gender gender)
        {
            return;
        }

        if (cmbBoxColumnHeader == @"Пол")
        {
            ViewModel.NewItem.Gender = gender;
        }
    }

    private void SetTextBoxValue(DataGridCellEditEndingEventArgs args, TextBox textBox)
    {
        string? text = textBox.Text;
        if (text is null)
        {
            args.Cancel = true;

            return;
        }

        var columnHeader = args.Column.Header.ToString();
        if (columnHeader is null)
        {
            return;
        }

        switch (columnHeader)
        {
            case @"Логин":
            {
                ViewModel.NewItem.User.Login = text;
                break;
            }

            case @"Фамилия":
            {
                ViewModel.NewItem.LastName = text;
                break;
            }

            case @"Имя":
            {
                ViewModel.NewItem.FirstName = text;
                break;
            }

            case @"Отчество":
            {
                ViewModel.NewItem.Patronymic = text;
                break;
            }

            case @"Электронная почта":
            {
                ViewModel.NewItem.EmailAddress = text;
                break;
            }

            default:
            {
                args.Cancel = true;

                return;
            }
        }
    }

    private void ChangeViewToAdd(bool isDirectConversion)
    {
        CommonActions.ChangeCommonViewToAdd(DgCustomers, BtnAddTextBlock, isDirectConversion);
        var columnQueryResult = DgCustomers.GetColumnByHeaderContent(@"Логин");
        if (!columnQueryResult.IsSuccessful)
        {
            MessageBox.Show(columnQueryResult.ErrorMessage);

            return;
        }

        columnQueryResult.Data.IsReadOnly = !isDirectConversion;
    }

    private void SetOrderCmbBoxValue(DataGridCellEditEndingEventArgs args, ComboBox comboBox)
    {
        var cmbBoxColumnHeader = args.Column.Header.ToString();
        if (cmbBoxColumnHeader is null)
        {
            return;
        }

        if (comboBox.SelectionBoxItem is not EnumerationMember enumerationMember)
        {
            return;
        }

        if (enumerationMember.Value is OrderStatus orderStatus)
        {
            if (cmbBoxColumnHeader == @"Статус")
            {
                ViewModel.NewOrder.Status = orderStatus;
            }
        }

        if (enumerationMember.Value is StatementOfWorkStatus statementOfWorkStatus)
        {
            if (cmbBoxColumnHeader == @"Статус ТЗ")
            {
                ViewModel.NewOrder.StatementOfWork.Status = statementOfWorkStatus;
            }
        }
    }

    private void SetOrderTextBoxValue(DataGridCellEditEndingEventArgs args, TextBox textBox)
    {
        string? text = textBox.Text;
        if (text is null)
        {
            args.Cancel = true;

            return;
        }

        var columnHeader = args.Column.Header.ToString();
        if (columnHeader is null)
        {
            return;
        }

        switch (columnHeader)
        {
            case @"Название":
            {
                ViewModel.NewOrder.Title = text;
                break;
            }

            case @"Текст":
            {
                ViewModel.NewOrder.Text = text;
                break;
            }

            case @"Название ТЗ":
            {
                ViewModel.NewOrder.StatementOfWork.Title = text;
                break;
            }

            case @"Текст ТЗ":
            {
                ViewModel.NewOrder.StatementOfWork.Text = text;
                break;
            }

            default:
            {
                args.Cancel = true;

                return;
            }
        }
    }

    private void ChangeViewToAddOrder(bool isDirectConversion)
    {
        CommonActions.ChangeCommonViewToAdd(DgOrders, BtnAddOrderTextBlock, isDirectConversion);
        foreach (var column in DgOrders.Columns)
        {
            column.Visibility = isDirectConversion ? Visibility.Collapsed : Visibility.Visible;
        }

        var titleColumnQueryResult = DgOrders.GetColumnByHeaderContent(@"Название");
        if (!titleColumnQueryResult.IsSuccessful)
        {
            MessageBox.Show(titleColumnQueryResult.ErrorMessage);

            return;
        }

        titleColumnQueryResult.Data.Visibility = Visibility.Visible;
        var textColumnQueryResult = DgOrders.GetColumnByHeaderContent(@"Текст");
        if (!textColumnQueryResult.IsSuccessful)
        {
            MessageBox.Show(textColumnQueryResult.ErrorMessage);

            return;
        }

        textColumnQueryResult.Data.Visibility = Visibility.Visible;
    }
}