using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using CleanModels;
using Gui.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Gui.Views.Main.Pages;

/// <summary>
/// Interaction logic for UsersPage.xaml.
/// </summary>
public partial class UsersPage : Page
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UsersPage"/> class.
    /// </summary>
    /// <param name="serviceProvider">Service provider.</param>
    public UsersPage(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        ViewModel = serviceProvider.GetRequiredService<UsersViewModel>();
        DataContext = ViewModel;
        Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);
    }

    private UsersViewModel ViewModel { get; }

    private bool CanSeeOnlyOwnValues => ViewModel.LoadedItemId != null;

    public void SetLoadedItemId(Guid? itemId)
    {
        ViewModel.SetLoadedItemIdCommand.Execute(itemId);
    }

    private void BeginningEdit(object? sender, DataGridBeginningEditEventArgs e)
    {
        CommonActions.BeginningEditHandler(DgUsers, e, ViewModel);
        ChangeViewToEdit(isDirectConversion: true);
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

        if (enumerationMember.Value is UserRole userRole && cmbBoxColumnHeader == @"Роль")
        {
            ViewModel.NewItem.Role = userRole;
        }

        if (enumerationMember.Value is Gender gender && cmbBoxColumnHeader == @"Пол")
        {
            if (ViewModel.Items[0].Role is UserRole.Customer)
            {
                ViewModel.NewItem.Customer.Gender = gender;
            }

            if (ViewModel.Items[0].Role is UserRole.Employee or UserRole.Administrator)
            {
                ViewModel.NewItem.Employee.Passport.Gender = gender;
            }
        }

        if (enumerationMember.Value is JobPosition jobPosition && cmbBoxColumnHeader == @"Должность")
        {
            ViewModel.NewItem.Employee.JobPosition = jobPosition;
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
            case @"Логин":
            {
                ViewModel.NewItem.Login = text;
                break;
            }

            case @"Фамилия":
            {
                if (ViewModel.Items[0].Role is UserRole.Customer)
                {
                    ViewModel.NewItem.Customer.LastName = text;
                }

                if (ViewModel.Items[0].Role is UserRole.Employee or UserRole.Administrator)
                {
                    ViewModel.NewItem.Employee.Passport.LastName = text;
                }

                break;
            }

            case @"Имя":
            {
                if (ViewModel.Items[0].Role is UserRole.Customer)
                {
                    ViewModel.NewItem.Customer.FirstName = text;
                }

                if (ViewModel.Items[0].Role is UserRole.Employee or UserRole.Administrator)
                {
                    ViewModel.NewItem.Employee.Passport.FirstName = text;
                }

                break;
            }

            case @"Отчество":
            {
                if (ViewModel.Items[0].Role is UserRole.Customer)
                {
                    ViewModel.NewItem.Customer.Patronymic = text;
                }

                if (ViewModel.Items[0].Role is UserRole.Employee or UserRole.Administrator)
                {
                    ViewModel.NewItem.Employee.Passport.Patronymic = text;
                }

                break;
            }

            case @"Электронная почта":
            {
                if (ViewModel.Items[0].Role is UserRole.Customer)
                {
                    ViewModel.NewItem.Customer.EmailAddress = text;
                }

                if (ViewModel.Items[0].Role is UserRole.Employee or UserRole.Administrator)
                {
                    ViewModel.NewItem.Employee.EmailAddress = text;
                }

                break;
            }

            case @"Серия паспорта":
            {
                bool textIsNumber = short.TryParse(text, out short number);
                if (textIsNumber)
                {
                    ViewModel.NewItem.Employee.PassportSeries = number;
                    ViewModel.NewItem.Employee.Passport.Series = number;
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
                    ViewModel.NewItem.Employee.PassportNumber = number;
                    ViewModel.NewItem.Employee.Passport.Number = number;
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
                    ViewModel.NewItem.Employee.Passport.BirthDate = dateTime;
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
                ViewModel.NewItem.Employee.Passport.Residence = text;
                break;
            }

            default:
            {
                e.Cancel = true;

                return;
            }
        }
    }

    private async void RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
    {
        IEditableCollectionView dgUsersItems = DgUsers.Items;
        if (!e.Row.IsNewItem && dgUsersItems.IsEditingItem)
        {
            if (!CommonActions.ConfirmNeedTo("Желаете ли вы сохранить изменения?"))
            {
                if (dgUsersItems.CanCancelEdit)
                {
                    dgUsersItems.CancelEdit();
                }

                ChangeViewToEdit(isDirectConversion: false);

                return;
            }

            if (dgUsersItems.CanCancelEdit)
            {
                dgUsersItems.CancelEdit();
            }

            dgUsersItems.CommitEdit();
            ChangeViewToEdit(isDirectConversion: false);
            ViewModel.UpdateCommand.Execute();
            /*await Task.Run(() => CommonActions.WaitingForCommandResult(ViewModel));
            ViewModel.ResetCommandResultCommand.Execute();*/
        }

        if (e.Row.IsNewItem && !dgUsersItems.IsEditingItem)
        {
            if (!CommonActions.ConfirmNeedTo("Желаете ли вы сохранить изменения?"))
            {
                dgUsersItems.CancelNew();

                return;
            }

            dgUsersItems.CancelNew();
            ViewModel.AddCommand.Execute();
            /*await Task.Run(() => CommonActions.WaitingForCommandResult(ViewModel));
            if (!ViewModel.IsSuccessfulCommandResult!.Value)
            {
                e.Cancel = true;

                return;
            }

            ViewModel.ResetCommandResultCommand.Execute();*/
        }
    }

    private void BtnDeleteClick(object sender, RoutedEventArgs e)
    {
        CommonActions.DeleteData(DgUsers, ViewModel.DeleteCommand, ViewModel);
    }

    private void BtnAddClick(object sender, RoutedEventArgs e)
    {
        if (DgUsers.CanUserAddRows)
        {
            ChangeViewToAdd(isDirectConversion: false);

            return;
        }

        ChangeViewToAdd(isDirectConversion: true);
    }

    private async void PasswordBoxPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is not PasswordBox passBox)
        {
            return;
        }

        ViewModel.SetPasswordCommand.Execute(passBox.Password);
        /*await Task.Run(() => CommonActions.WaitingForCommandResult(ViewModel));
        ViewModel.ResetCommandResultCommand.Execute();*/
    }

    private void TextBoxFilterPreviewKeyUp(object sender, KeyEventArgs e)
    {
        string text = TextBoxFilter?.SearchTerm?.ToLower() ?? string.Empty;
        bool Predicate(PropertyInfo property) => property.Name is not "Id" and not "Customer" and not "Employee";
        foreach (var item in ViewModel.Items)
        {
            DataGridFilter.FilterWithEnum(item, DgUsers, Predicate, text);
        }
    }

    /*private void SetValueToUserByRole<T>(T customerProperty, T employeeProperty, T value)
    {
        if (ViewModel.Items[0].Role is UserRole.Customer)
        {
            customerProperty = value;
        }

        if (ViewModel.Items[0].Role is UserRole.Employee or UserRole.Administrator)
        {
            employeeProperty = value;
        }
    }*/

    /*private void RowDoubleClick(object sender, MouseButtonEventArgs e)
    {
        DgUsers.SelectedItem = sender;
        DgUsers.SelectedItems.Add(DgUsers.SelectedItem);
    }*/

    private void ChangeViewToAdd(bool isDirectConversion)
    {
        CommonActions.ChangeCommonViewToAdd(DgUsers, BtnAddTextBlock, isDirectConversion);
        var loginColumnQueryResult = DgUsers.GetColumnByHeaderContent(@"Логин");
        if (!loginColumnQueryResult.IsSuccessful)
        {
            MessageBox.Show(loginColumnQueryResult.ErrorMessage);

            return;
        }

        loginColumnQueryResult.Data.IsReadOnly = !isDirectConversion;
        var roleColumnQueryResult = DgUsers.GetColumnByHeaderContent(@"Роль");
        if (!roleColumnQueryResult.IsSuccessful)
        {
            MessageBox.Show(roleColumnQueryResult.ErrorMessage);

            return;
        }

        roleColumnQueryResult.Data.IsReadOnly = !isDirectConversion;
        ChangeViewToEdit(isDirectConversion);
    }

    private void ChangeViewToEdit(bool isDirectConversion)
    {
        var currentFirstColumnHeaderContent = @"Хеш пароля";
        var newFirstColumnVisibility = isDirectConversion ? Visibility.Collapsed : Visibility.Visible;
        var currentSecondColumnHeaderContent = @"Соль пароля";
        var newSecondColumnVisibility = isDirectConversion ? Visibility.Collapsed : Visibility.Visible;
        var currentThirdColumnHeaderContent = @"Пароль";
        var newThirdColumnVisibility = isDirectConversion ? Visibility.Visible : Visibility.Collapsed;
        if (CanSeeOnlyOwnValues)
        {
            return;
        }

        CommonActions.ChangeColumnVisibility(DgUsers, currentFirstColumnHeaderContent, newFirstColumnVisibility);
        CommonActions.ChangeColumnVisibility(DgUsers, currentSecondColumnHeaderContent, newSecondColumnVisibility);
        //CommonActions.ChangeColumnVisibility(DgUsers, currentThirdColumnHeaderContent, newThirdColumnVisibility);
    }
}