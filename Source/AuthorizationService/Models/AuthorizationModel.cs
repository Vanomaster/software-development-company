namespace AuthorizationService;

/// <summary>
/// Model of user for authorization.
/// </summary>
public class AuthorizationModel
{
    /// <summary>
    /// User name.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// User password.
    /// </summary>
    public string Password { get; set; } = string.Empty;
}