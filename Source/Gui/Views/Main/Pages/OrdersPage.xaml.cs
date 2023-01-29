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

namespace Gui.Views;

/// <summary>
/// Interaction logic for OrdersPage.xaml.
/// </summary>
public partial class OrdersPage : Page
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OrdersPage"/> class.
    /// </summary>
    /// <param name="serviceProvider">Service provider.</param>
    public OrdersPage(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        ViewModel = serviceProvider.GetRequiredService<OrdersViewModel>();
        DataContext = ViewModel;
        Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);
    }

    private OrdersViewModel ViewModel { get; }

    private void BeginningEdit(object? sender, DataGridBeginningEditEventArgs e)
    {
        CommonActions.BeginningEditHandler(DgOrders, e, ViewModel);
    }

    private void CellEditEnding(object? sender, DataGridCellEditEndingEventArgs e)
    {
        if (e.EditingElement is ComboBox comboBox)
        {
            SetCmbBoxValue(e, comboBox);

            return;
        }

        if (e.EditingElement is TextBox textBox)
        {
            SetTextBoxValue(e, textBox);
        }
    }

    private void RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
    {
        CommonActions.RowEditEndingHandler(DgOrders, e, ViewModel, ViewModel.UpdateCommand, ViewModel.AddCommand);
    }

    /*private void BtnDeleteClick(object sender, RoutedEventArgs e)
    {
        CommonActions.DeleteData(DgOrders, ViewModel.DeleteCommand, ViewModel);
    }

    private void BtnAddClick(object sender, RoutedEventArgs e)
    {
        if (DgOrders.CanUserAddRows)
        {
            ChangeViewToAdd(isDirectConversion: false);

            return;
        }

        ChangeViewToAdd(isDirectConversion: true);
    }*/

    private void TextBoxFilterPreviewKeyUp(object sender, KeyEventArgs e)
    {
        string text = TextBoxFilter?.SearchTerm?.ToLower() ?? string.Empty;
        bool Predicate(PropertyInfo property) => property.Name is not "Id" and not "CustomerId" and not "Customer"
            and not "Employees" and not "StatementOfWork";
        bool AdditionalPredicate(PropertyInfo property) => property.Name is not "Id" and not "OrderId" and not "Order";
        bool UserPredicate(PropertyInfo property) => property.Name is "Login";
        foreach (var item in ViewModel.Items)
        {
            DataGridFilter.FilterWithEnumAndAdditional(item, item.StatementOfWork, item.Customer.User, DgOrders, Predicate, AdditionalPredicate, UserPredicate, text);
        }
    }

    private void DgOrdersSelectionChanged(object sender, RoutedEventArgs e)
    {
        CommonActions.SelectionChangedHandler(
            DgOrders,
            ViewModel.RefreshEmployeesCommand,
            ViewModel);
    }

    private void EmployeeBeginningEdit(object? sender, DataGridBeginningEditEventArgs e)
    {
        CommonActions.BeginningEditAdditionalHandler<Order, Employee>(DgEmployees, e, ViewModel);
    }

    private void EmployeeCellEditEnding(object? sender, DataGridCellEditEndingEventArgs e)
    {
        if (e.EditingElement is ComboBox comboBox)
        {
            SetEmployeeCmbBoxValue(e, comboBox);

            return;
        }

        if (e.EditingElement is TextBox textBox)
        {
            SetEmployeeTextBoxValue(e, textBox);
        }
    }

    private void EmployeeRowEditEnding(object? sender, DataGridRowEditEndingEventArgs e)
    {
        CommonActions.RowEditEndingHandler(DgEmployees, e, ViewModel, ViewModel.UpdateEmployeeCommand, ViewModel.AddEmployeeCommand);
    }

    private void BtnDeleteEmployeesClick(object sender, RoutedEventArgs e)
    {
        CommonActions.DeleteData(DgEmployees, ViewModel.DeleteEmployeeCommand, ViewModel);
    }

    private void BtnAddEmployeesClick(object sender, RoutedEventArgs e)
    {
        if (DgEmployees.CanUserAddRows)
        {
            ChangeViewToAddEmployee(isDirectConversion: false);

            return;
        }

        ChangeViewToAddEmployee(isDirectConversion: true);
    }

    private void TextBoxFilterEmployeesPreviewKeyUp(object sender, KeyEventArgs e)
    {
        string text = TextBoxFilterEmployees?.SearchTerm?.ToLower() ?? string.Empty;
        bool Predicate(PropertyInfo property) => property.Name is not "Id" and not "Orders";
        bool AdditionalPredicate(PropertyInfo property) => property.Name is "Login";
        bool PassportPredicate(PropertyInfo property) => property.Name is not "Employee";
        foreach (var item in ViewModel.Employees)
        {
            DataGridFilter.FilterWithEnumAndAdditional(item, item.User, item.Passport, DgEmployees, Predicate, AdditionalPredicate, PassportPredicate, text);
        }
    }

    private void SetCmbBoxValue(DataGridCellEditEndingEventArgs e, ComboBox comboBox)
    {
        var cmbBoxColumnHeader = e.Column.Header.ToString();
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
                ViewModel.NewItem.Status = orderStatus;
            }
        }

        if (enumerationMember.Value is StatementOfWorkStatus statementOfWorkStatus)
        {
            if (cmbBoxColumnHeader == @"Статус ТЗ")
            {
                ViewModel.NewItem.StatementOfWork.Status = statementOfWorkStatus;
            }
        }
    }

    private void SetTextBoxValue(DataGridCellEditEndingEventArgs e, TextBox textBox)
    {
        string? text = textBox.Text;
        if (text is null)
        {
            e.Cancel = true;

            return;
        }

        var columnHeader = e.Column.Header.ToString();
        if (columnHeader is null)
        {
            return;
        }

        switch (columnHeader)
        {
            case @"Номер":
            {
                bool textIsNumber = int.TryParse(text, out int number);
                if (textIsNumber)
                {
                    ViewModel.NewItem.Number = number;
                }

                if (!textIsNumber)
                {
                    MessageBox.Show(
                        @"Введён некорректный номер заказа. Номер заказа должен быть в виде положительного числа и не более 2 147 483 647.",
                        @"Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }

                break;
            }

            case @"Название":
            {
                ViewModel.NewItem.Title = text;
                break;
            }

            case @"Текст":
            {
                ViewModel.NewItem.Text = text;
                break;
            }

            case @"Стоимость":
            {
                bool textIsNumber = int.TryParse(text, out int number);
                if (textIsNumber)
                {
                    ViewModel.NewItem.Cost = number;
                }

                if (!textIsNumber)
                {
                    ViewModel.NewItem.Cost = null;
                    MessageBox.Show(
                        @"Введена некорректная стоимость. Стоимость должна быть в виде положительного числа и не более 2 147 483 647.",
                        @"Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }

                break;
            }

            case @"Название ТЗ":
            {
                ViewModel.NewItem.StatementOfWork.Title = text;
                break;
            }

            case @"Текст ТЗ":
            {
                ViewModel.NewItem.StatementOfWork.Text = text;
                break;
            }

            default:
            {
                e.Cancel = true;

                return;
            }
        }
    }

    /*private void ChangeViewToAdd(bool isDirectConversion)
    {
        CommonActions.ChangeCommonViewToAdd(DgOrders, BtnAddTextBlock, isDirectConversion);
        foreach (var column in DgOrders.Columns)
        {
            column.Visibility = isDirectConversion ? Visibility.Collapsed : Visibility.Visible;
        }

        var columnQueryResult = DgOrders.GetColumnByHeaderContent(@"Номер");
        if (!columnQueryResult.IsSuccessful)
        {
            MessageBox.Show(columnQueryResult.ErrorMessage);

            return;
        }

        columnQueryResult.Data.IsReadOnly = !isDirectConversion;
        columnQueryResult.Data.Visibility = Visibility.Visible;
    }*/

    private void SetEmployeeCmbBoxValue(DataGridCellEditEndingEventArgs e, ComboBox comboBox)
    {
        var cmbBoxColumnHeader = e.Column.Header.ToString();
        if (cmbBoxColumnHeader is null)
        {
            return;
        }

        if (comboBox.SelectionBoxItem is not EnumerationMember enumerationMember)
        {
            return;
        }

        if (enumerationMember.Value is Gender gender)
        {
            if (cmbBoxColumnHeader == @"Пол")
            {
                ViewModel.NewEmployee.Passport.Gender = gender;
            }
        }

        if (enumerationMember.Value is JobPosition jobPosition)
        {
            if (cmbBoxColumnHeader == @"Должность")
            {
                ViewModel.NewEmployee.JobPosition = jobPosition;
            }
        }
    }

    private void SetEmployeeTextBoxValue(DataGridCellEditEndingEventArgs e, TextBox textBox)
    {
        string? text = textBox.Text;
        if (text is null)
        {
            e.Cancel = true;

            return;
        }

        var columnHeader = e.Column.Header.ToString();
        if (columnHeader is null)
        {
            return;
        }

        switch (columnHeader)
        {
            case @"Логин":
            {
                ViewModel.NewEmployee.User.Login = text;
                break;
            }

            case @"Серия паспорта":
            {
                bool textIsNumber = short.TryParse(text, out short number);
                if (textIsNumber)
                {
                    ViewModel.NewEmployee.PassportSeries = number;
                    ViewModel.NewEmployee.Passport.Series = number;
                }

                if (!textIsNumber)
                {
                    MessageBox.Show(
                        @"Введена некорректная серия паспорта. Серия паспорта должна быть в виде четырёхзначного числа.",
                        @"Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }

                break;
            }

            case @"Номер паспорта":
            {
                bool textIsNumber = int.TryParse(text, out int number);
                if (textIsNumber)
                {
                    ViewModel.NewEmployee.PassportNumber = number;
                    ViewModel.NewEmployee.Passport.Number = number;
                }

                if (!textIsNumber)
                {
                    MessageBox.Show(
                        @"Введён некорректный номер паспорта. Номер паспорта должен быть в виде шестизначного числа.",
                        @"Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }

                break;
            }

            case @"Фамилия":
            {
                ViewModel.NewEmployee.Passport.LastName = text;
                break;
            }

            case @"Имя":
            {
                ViewModel.NewEmployee.Passport.FirstName = text;
                break;
            }

            case @"Отчество":
            {
                ViewModel.NewEmployee.Passport.Patronymic = text;
                break;
            }

            case @"Дата рождения":
            {
                const string dateFormat = "dd.MM.yyyy";
                bool isDate = DateTime.TryParseExact(
                    text,
                    dateFormat,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AllowWhiteSpaces,
                    out var dateTime);

                if (isDate)
                {
                    ViewModel.NewEmployee.Passport.BirthDate = dateTime;
                }

                if (!isDate)
                {
                    const string dateFormatRus = @"ДД.ММ.ГГГГ";
                    MessageBox.Show(
                        @"Введена некорректная дата рождения. Дата рождения должна быть в формате " + dateFormatRus + ".",
                        @"Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }

                break;
            }

            case @"Место жительства":
            {
                ViewModel.NewEmployee.Passport.Residence = text;
                break;
            }

            case @"Электронная почта":
            {
                ViewModel.NewEmployee.EmailAddress = text;
                break;
            }

            default:
            {
                e.Cancel = true;

                return;
            }
        }
    }

    private void ChangeViewToAddEmployee(bool isDirectConversion)
    {
        CommonActions.ChangeCommonViewToAdd(DgEmployees, BtnAddEmployeeTextBlock, isDirectConversion);
        foreach (var column in DgEmployees.Columns)
        {
            /*if (column.Header.ToString() is "Серия паспорта" or "Номер паспорта" or "Место жительства")
            {
                continue;
            }*/

            column.Visibility = isDirectConversion ? Visibility.Collapsed : Visibility.Visible;
        }

        var columnQueryResult = DgEmployees.GetColumnByHeaderContent(@"Логин");
        if (!columnQueryResult.IsSuccessful)
        {
            MessageBox.Show(columnQueryResult.ErrorMessage);

            return;
        }

        columnQueryResult.Data.IsReadOnly = !isDirectConversion;
        columnQueryResult.Data.Visibility = Visibility.Visible;
    }
}