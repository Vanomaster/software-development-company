using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CleanModels;
using Dal;
using Dal.Entities;
using Microsoft.EntityFrameworkCore;
using Queries;
using Queries.Base;

namespace AuthorizationService;

/// <summary>
/// Authorization query.
/// </summary>
public class AuthorizationQuery : AsyncQueryBase<AuthorizationModel, UserPrincipal>
{
    private readonly Regex usernameNormalizer = new (
        pattern: "^[\\wа-яА-ЯёЁ]+$",
        options: RegexOptions.Compiled | RegexOptions.Singleline);

    /// <summary>
    /// Initializes a new instance of the <see cref="OrdersQuery"/> class.
    /// </summary>
    /// <param name="contextFactory">Context factory.</param>
    /// <param name="hashMachine">Hash machine.</param>
    public AuthorizationQuery(IDbContextFactory<Context> contextFactory, HashMachine hashMachine)
        : base(contextFactory)
    {
        HashMachine = hashMachine;
    }

    private HashMachine HashMachine { get; }

    /// <inheritdoc/>
    protected override async Task<QueryResult<UserPrincipal>> ExecuteCoreAsync(AuthorizationModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Username))
        {
            return GetFailedResult(@"Не введено имя пользователя");
        }

        if (string.IsNullOrWhiteSpace(model.Password))
        {
            return GetFailedResult(@"Не введён пароль");
        }

        string username = GetNormalizedUsername(model.Username);
        if (username == string.Empty)
        {
            return GetFailedResult(@"Недопустимое имя пользователя");
        }

        var identifiedUsers = Context.Users.Where(user => user.Login == username);
        if (!identifiedUsers.Any())
        {
            return GetFailedResult(@"Пользователь с таким именем не зарегистрирован");
        }

        var user = await identifiedUsers.FirstAsync();
        var verifyingPasswordResult = HashMachine.VerifyPassword(model.Password, user.PasswordSalt, user.PasswordHash);
        if (!verifyingPasswordResult.IsSuccessful)
        {
            return GetFailedResult(verifyingPasswordResult.ErrorMessage);
        }

        bool userIsAuthenticated = verifyingPasswordResult.Data;
        if (!userIsAuthenticated)
        {
            return GetFailedResult(@"Неверный пароль пользователя");
        }

        var userData = await GetUserData(user);
        var userPrincipal = new UserPrincipal(user.Id, user.Login, userData, user.Role);

        return GetSuccessfulResult(userPrincipal);
    }

    private async Task<IUserData> GetUserData(User user)
    {
        return user.Role switch
        {
            UserRole.Administrator or UserRole.Employee => await GetEmployeeData(user.Id),
            UserRole.Customer => await GetCustomerData(user.Id),
            _ => null!
        };
    }

    private async Task<CustomerData> GetCustomerData(Guid userId)
    {
        var customerData = await Context.Customers
            .Where(customer => customer.Id == userId)
            .Select(customer => new CustomerData
            {
                LastName = customer.LastName,
                FirstName = customer.FirstName,
                Patronymic = customer.Patronymic,
                Gender = customer.Gender,
                EmailAddress = customer.EmailAddress,
            })
            .FirstOrDefaultAsync();

        return customerData;
    }

    private async Task<EmployeeData> GetEmployeeData(Guid userId)
    {
        var employeeData = await Context.Employees
            .Where(employee => employee.Id == userId)
            .Select(employee => new EmployeeData
            {
                PassportSeries = employee.PassportSeries,
                PassportNumber = employee.PassportNumber,
                JobPosition = employee.JobPosition,
                EmailAddress = employee.EmailAddress,
                LastName = employee.Passport.LastName,
                FirstName = employee.Passport.FirstName,
                Patronymic = employee.Passport.Patronymic,
                Gender = employee.Passport.Gender,
                BirthDate = employee.Passport.BirthDate,
                Residence = employee.Passport.Residence,
            })
            .FirstOrDefaultAsync();

        return employeeData;
    }

    private string GetNormalizedUsername(string username)
    {
        /*var match = usernameNormalizer.Match($"{username}@");
        if (!match.Success && match.Groups.Count != 2)
        {
            return string.Empty;
        }

        string normalizedUsername = match.Groups[1].Value;*/

        var match = usernameNormalizer.Match(username);
        if (!match.Success)
        {
            return string.Empty;
        }

        string normalizedUsername = username.Trim();

        return normalizedUsername;
    }
}