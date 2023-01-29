using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AuthorizationService;
using CleanModels;
using Commands;
using Commands.Base;
using Dal.Entities;
using Gui.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Prism.Mvvm;
using Queries;
using Queries.Base;

namespace Gui.Models;

public class UsersModel : BindableBase
{
    public readonly ObservableCollection<User> Items = new ();

    /// <summary>
    /// Initializes a new instance of the <see cref="UsersModel"/> class.
    /// </summary>
    /// <param name="serviceProvider">Service provider.</param>
    /// <param name="dataValidator">Data validator.</param>
    public UsersModel(IServiceProvider serviceProvider, DataValidator dataValidator)
    {
        ServiceProvider = serviceProvider;
        DataValidator = dataValidator;
    }

    private IServiceProvider ServiceProvider { get; }

    private DataValidator DataValidator { get; }

    public async Task<QueryResult<List<User>>> GetItemsAsync(Guid? itemId)
    {
        var query = ServiceProvider.GetRequiredService<UsersQuery>();
        var queryResult = itemId.HasValue
            ? await query.ExecuteAsync(new List<Guid> { itemId.Value })
            : await query.ExecuteAsync();

        if (!queryResult.IsSuccessful)
        {
            return new QueryResult<List<User>>(queryResult.ErrorMessage);
        }

        return new QueryResult<List<User>>(queryResult.Data);
    }

    public void RefreshItems(List<User> newItems)
    {
        var newItemsOrderedByLogin = newItems.OrderBy(item => item.Login);
        Items.Clear();
        Items.AddRange(newItemsOrderedByLogin);
        RaisePropertyChanged("Items");
    }

    public CommandResult ValidateUser(User user, Guid? loadedItemId)
    {
        var validateUserByRoleCommandResults = ValidateUserByRole(user, loadedItemId);
        string validationErrors = validateUserByRoleCommandResults
            .Where(commandResult => !commandResult.IsSuccessful)
            .Aggregate(string.Empty, (current, commandResult) => current + commandResult.ErrorMessage + "\n");

        if (validationErrors != string.Empty)
        {
            return new CommandResult(validationErrors);
        }

        return new CommandResult();
    }

    private List<CommandResult> ValidateUserByRole(User user, Guid? loadedItemId)
    {
        var commandResults = new List<CommandResult>();
        var isValidLoginCommandResult = DataValidator.IsValidLogin(user.Login);
        commandResults.Add(isValidLoginCommandResult);
        if (user.Id == default && Items.Any(item => item.Login == user.Login))
        {
            commandResults.Add(new CommandResult(@"Пользователь с таким логином уже существует"));
        }

        if (loadedItemId is null)
        {
            return commandResults;
        }

        if (Items[0].Role is UserRole.Customer)
        {
            var validateCustomerCommandResult = ValidateCustomer(user.Customer);
            commandResults.Add(validateCustomerCommandResult);
        }

        if (Items[0].Role is UserRole.Employee or UserRole.Administrator)
        {
            var validateCustomerCommandResult = ValidateEmployee(user.Employee);
            commandResults.Add(validateCustomerCommandResult);
        }

        return commandResults;
    }

    private CommandResult ValidateCustomer(Customer customer)
    {
        var firstNameValidationCommandResult = DataValidator.IsValidFirstName(customer.FirstName);
        var lastNameValidationCommandResult = DataValidator.IsValidLastName(customer.LastName);
        var patronymicValidationCommandResult = DataValidator.IsValidPatronymic(customer.Patronymic);
        var emailValidationCommandResult = DataValidator.IsValidEmail(customer.EmailAddress);
        var commandResults = new List<CommandResult>
        {
            firstNameValidationCommandResult,
            lastNameValidationCommandResult,
            patronymicValidationCommandResult,
            emailValidationCommandResult,
        };

        string validationErrors = commandResults
            .Where(commandResult => !commandResult.IsSuccessful)
            .Aggregate(string.Empty, (current, commandResult) => current + commandResult.ErrorMessage + "\n");

        if (validationErrors != string.Empty)
        {
            return new CommandResult(validationErrors);
        }

        return new CommandResult();
    }

    private CommandResult ValidateEmployee(Employee employee)
    {
        var passportSeriesValidationCommandResult = DataValidator.IsValidPassportSeries(employee.PassportSeries);
        var passportNumberValidationCommandResult = DataValidator.IsValidPassportNumber(employee.PassportNumber);
        var emailValidationCommandResult = DataValidator.IsValidEmail(employee.EmailAddress);
        var lastNameValidationCommandResult = DataValidator.IsValidLastName(employee.Passport.LastName);
        var firstNameValidationCommandResult = DataValidator.IsValidFirstName(employee.Passport.FirstName);
        var patronymicValidationCommandResult = DataValidator.IsValidPatronymic(employee.Passport.Patronymic);
        var birthDateValidationCommandResult = DataValidator.IsValidBirthDate(employee.Passport.BirthDate);
        var residenceValidationCommandResult = DataValidator.IsValidResidence(employee.Passport.Residence);
        var commandResults = new List<CommandResult>
        {
            passportSeriesValidationCommandResult,
            passportNumberValidationCommandResult,
            emailValidationCommandResult,
            lastNameValidationCommandResult,
            firstNameValidationCommandResult,
            patronymicValidationCommandResult,
            birthDateValidationCommandResult,
            residenceValidationCommandResult,
        };

        string validationErrors = commandResults
            .Where(commandResult => !commandResult.IsSuccessful)
            .Aggregate(string.Empty, (current, commandResult) => current + commandResult.ErrorMessage + "\n");

        if (validationErrors != string.Empty)
        {
            return new CommandResult(validationErrors);
        }

        return new CommandResult();
    }

    public async Task<CommandResult> AddItemAsync(User itemToAdd)
    {
        var command = ServiceProvider.GetRequiredService<AddEntityCommand>();
        var commandResult = await command.ExecuteAsync(new List<IEntity> { itemToAdd });
        if (!commandResult.IsSuccessful)
        {
            return new CommandResult(commandResult.ErrorMessage);
        }

        return new CommandResult();
    }

    public void AddItemToView(User itemToAdd)
    {
        Items.Add(itemToAdd);
        RaisePropertyChanged("Items");
    }

    public CommandResult<User> GetActualItem(User itemToUpdate, Guid? loadedItemId)
    {
        var item = Items.SingleOrDefault(item => item.Id == itemToUpdate.Id);
        if (item is null)
        {
            return new CommandResult<User>("Неопознанная ошибка");
        }

        var actualItem = new User
        {
            Id = itemToUpdate.Id,
            Login = string.IsNullOrWhiteSpace(itemToUpdate.Login)
                ? item.Login
                : itemToUpdate.Login,
            PasswordHash = string.IsNullOrWhiteSpace(itemToUpdate.PasswordHash)
                ? item.PasswordHash
                : itemToUpdate.PasswordHash,
            PasswordSalt = string.IsNullOrWhiteSpace(itemToUpdate.PasswordSalt)
                ? item.PasswordSalt
                : itemToUpdate.PasswordSalt,
            Role = itemToUpdate.Role == 0
                ? item.Role
                : itemToUpdate.Role,
        };

        if (loadedItemId is null)
        {
            actualItem.Customer = null!;
            actualItem.Employee = null!;

            return new CommandResult<User>(actualItem);
        }

        if (Items[0].Role is UserRole.Customer)
        {
            actualItem.Customer = new Customer
            {
                Id = itemToUpdate.Id,
                LastName = string.IsNullOrWhiteSpace(itemToUpdate.Customer.LastName)
                    ? item.Customer.LastName
                    : itemToUpdate.Customer.LastName,
                FirstName = string.IsNullOrWhiteSpace(itemToUpdate.Customer.FirstName)
                    ? item.Customer.FirstName
                    : itemToUpdate.Customer.FirstName,
                Patronymic = string.IsNullOrWhiteSpace(itemToUpdate.Customer.Patronymic)
                    ? item.Customer.Patronymic
                    : itemToUpdate.Customer.Patronymic,
                Gender = itemToUpdate.Customer.Gender == 0
                    ? item.Customer.Gender
                    : itemToUpdate.Customer.Gender,
                EmailAddress = string.IsNullOrWhiteSpace(itemToUpdate.Customer.EmailAddress)
                    ? item.Customer.EmailAddress
                    : itemToUpdate.Customer.EmailAddress,
            };
        }

        if (Items[0].Role is UserRole.Employee or UserRole.Administrator)
        {
            actualItem.Employee = new Employee
            {
                Id = itemToUpdate.Id,
                PassportSeries = itemToUpdate.Employee.PassportSeries == 0
                    ? item.Employee.PassportSeries
                    : itemToUpdate.Employee.PassportSeries,
                PassportNumber = itemToUpdate.Employee.PassportNumber == 0
                    ? item.Employee.PassportNumber
                    : itemToUpdate.Employee.PassportNumber,
                JobPosition = itemToUpdate.Employee.JobPosition == 0
                    ? item.Employee.JobPosition
                    : itemToUpdate.Employee.JobPosition,
                EmailAddress = string.IsNullOrWhiteSpace(itemToUpdate.Employee.EmailAddress)
                    ? item.Employee.EmailAddress
                    : itemToUpdate.Employee.EmailAddress,
                Passport = new Passport
                {
                    Series = itemToUpdate.Employee.PassportSeries == 0
                        ? item.Employee.PassportSeries
                        : itemToUpdate.Employee.PassportSeries,
                    Number = itemToUpdate.Employee.PassportNumber == 0
                        ? item.Employee.PassportNumber
                        : itemToUpdate.Employee.PassportNumber,
                    LastName = string.IsNullOrWhiteSpace(itemToUpdate.Employee.Passport.LastName)
                        ? item.Employee.Passport.LastName
                        : itemToUpdate.Employee.Passport.LastName,
                    FirstName = string.IsNullOrWhiteSpace(itemToUpdate.Employee.Passport.FirstName)
                        ? item.Employee.Passport.FirstName
                        : itemToUpdate.Employee.Passport.FirstName,
                    Patronymic = string.IsNullOrWhiteSpace(itemToUpdate.Employee.Passport.Patronymic)
                        ? item.Employee.Passport.Patronymic
                        : itemToUpdate.Employee.Passport.Patronymic,
                    Gender = itemToUpdate.Employee.Passport.Gender == 0
                        ? item.Employee.Passport.Gender
                        : itemToUpdate.Employee.Passport.Gender,
                    BirthDate = itemToUpdate.Employee.Passport.BirthDate == default
                        ? item.Employee.Passport.BirthDate
                        : itemToUpdate.Employee.Passport.BirthDate,
                    Residence = string.IsNullOrWhiteSpace(itemToUpdate.Employee.Passport.Residence)
                        ? item.Employee.Passport.Residence
                        : itemToUpdate.Employee.Passport.Residence,
                },
            };
        }

        return new CommandResult<User>(actualItem);
    }

    public async Task<CommandResult> UpdateItemAsync(User itemToUpdate)
    {
        var command = ServiceProvider.GetRequiredService<AddOrUpdateEntityCommand>();
        var commandResult = await command.ExecuteAsync(new List<IEntity> { itemToUpdate });
        if (!commandResult.IsSuccessful)
        {
            return new CommandResult(commandResult.ErrorMessage);
        }

        return new CommandResult();
    }

    public void UpdateItemInView(User itemToUpdate)
    {
        var item = Items.SingleOrDefault(item => item.Id == itemToUpdate.Id);
        Items.Remove(item);
        Items.Add(itemToUpdate);
        RaisePropertyChanged("Items");
    }

    public async Task<CommandResult> DeleteItemAsync(List<User> itemsToDelete)
    {
        foreach (var item in itemsToDelete)
        {
            if (item.Role == UserRole.Customer)
            {
                var query = ServiceProvider.GetRequiredService<OrdersQueryByCustomerLogin>();
                var queryResult = await query.ExecuteAsync(item.Login);
                if (!queryResult.IsSuccessful)
                {
                    return new CommandResult(queryResult.ErrorMessage);
                }

                if (!queryResult.Data.IsNullOrEmpty())
                {
                    return new CommandResult(@"У заказчика есть заказы. Необходимо удалить все заказы перед удалением заказчика.");
                }
            }

            if (item.Role == UserRole.Employee)
            {
                var query = ServiceProvider.GetRequiredService<OrdersQueryByEmployeeLogin>();
                var queryResult = await query.ExecuteAsync(item.Login);
                if (!queryResult.IsSuccessful)
                {
                    return new CommandResult(queryResult.ErrorMessage);
                }

                if (!queryResult.Data.IsNullOrEmpty())
                {
                    return new CommandResult(@"К сотруднику прикреплены заказы. Необходимо открепить все заказы перед удалением сотрудника.");
                }
            }
        }

        var command = ServiceProvider.GetRequiredService<DeleteEntityCommand>();
        var commandResult = await command.ExecuteAsync(itemsToDelete);
        if (!commandResult.IsSuccessful)
        {
            return new CommandResult(commandResult.ErrorMessage);
        }

        return new CommandResult();
    }

    public void DeleteItemFromView(List<User> itemsToDelete)
    {
        foreach (var item in itemsToDelete)
        {
            Items.Remove(item);
        }

        RaisePropertyChanged("Items");
    }


    /*public CommandResult VerifyUserPassword(string password)
    {
        var hashMachine = ServiceProvider.GetRequiredService<HashMachine>();
        hashMachine.VerifyPassword();
    }*/

    public CommandResult SetUserPasswordProperties(string password, User user)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            user.PasswordSalt = string.Empty;
            user.PasswordHash = string.Empty;

            return new CommandResult();
        }

        var hashMachine = ServiceProvider.GetRequiredService<HashMachine>();
        string passwordSalt = hashMachine.GenerateSalt();
        var passwordHashGeneratingResult = hashMachine.GeneratePasswordHash(password, passwordSalt);
        if (!passwordHashGeneratingResult.IsSuccessful)
        {
            return new CommandResult(passwordHashGeneratingResult.ErrorMessage);
        }

        string passwordHash = passwordHashGeneratingResult.Data;
        user.PasswordSalt = passwordSalt;
        user.PasswordHash = passwordHash;

        return new CommandResult();
    }
}