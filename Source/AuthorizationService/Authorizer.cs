using System;
using System.Threading.Tasks;
using CleanModels;
using Queries.Base;

namespace AuthorizationService;

/// <summary>
/// Authorizer.
/// </summary>
public class Authorizer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Authorizer"/> class.
    /// </summary>
    /// <param name="authorizationQuery">Authorization query.</param>
    public Authorizer(AuthorizationQuery authorizationQuery)
    {
        AuthorizationQuery = authorizationQuery;
    }

    private AuthorizationQuery AuthorizationQuery { get; }

    /// <summary>
    /// Authorize.
    /// </summary>
    /// <param name="model">Model of user for authentication.</param>
    /// <returns>Result of authorization.</returns>
    public async Task<QueryResult<UserPrincipal>> Authorize(AuthorizationModel model)
    {
        var authorizationResult = await AuthorizationQuery.ExecuteAsync(model);
        if (!authorizationResult.IsSuccessful)
        {
            return new QueryResult<UserPrincipal>(authorizationResult.ErrorMessage);
        }

        return new QueryResult<UserPrincipal>(data: authorizationResult.Data);
    }

    /// <summary>
    /// Logout.
    /// </summary>
    public void Logout()
    {
        throw new NotImplementedException();
    }
}