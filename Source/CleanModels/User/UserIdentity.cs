using System;
using System.Security.Principal;

namespace CleanModels;

/// <summary>
/// User identity data.
/// </summary>
public class UserIdentity/* : IIdentity*/
{
    public UserIdentity()
    {
    }

    public UserIdentity(Guid id, string login)
    {
        Id = id;
        Login = login;
    }

    public UserIdentity(Guid id, string login, IUserData data)
    {
        Id = id;
        Login = login;
        Data = data;
    }

    /// <summary>
    /// User id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// User login.
    /// </summary>
    public string Login { get; set; }

    /// <summary>
    /// User data.
    /// </summary>
    public IUserData Data { get; set; }

    /// <inheritdoc/>
    public bool IsAuthenticated => Id != null;

    /*/// <inheritdoc/>
    public string Name => null;

    /// <inheritdoc/>
    public string AuthenticationType { get; } = "Password";*/
}