using CleanModels;

namespace AuthorizationService;

/// <summary>
/// .
/// </summary>
public class AuthorizationQueryResult
{
    public AuthorizationQueryResult(string message, UserPrincipal userPrincipal)
    {
        Message = message;
        UserPrincipal = userPrincipal;
    }

    /// <summary>
    /// .
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// .
    /// </summary>
    public UserPrincipal UserPrincipal { get; }
}