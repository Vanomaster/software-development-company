using System;
using System.Security.Principal;
using System.Text.Json.Serialization;

namespace CleanModels;

/// <summary>
/// .
/// </summary>
public class UserPrincipal/* : IPrincipal*/
{
    public UserPrincipal()
    {
    }

    public UserPrincipal(Guid id, string login, UserRole role)
    {
        TypedIdentity = new UserIdentity(id, login);
        Role = role;
    }

    public UserPrincipal(Guid id, string login, IUserData data, UserRole role)
    {
        TypedIdentity = new UserIdentity(id, login, data);
        Role = role;
    }

    /// <summary>
    /// Identity.
    /// </summary>
    public UserIdentity TypedIdentity { get; set; }

    /*/// <inheritdoc/>
    public IIdentity Identity => TypedIdentity;*/

    /// <summary>
    /// User role.
    /// </summary>
    public UserRole Role { get; set; }

    /// <summary>
    /// Check if user has at least one of roles.
    /// </summary>
    /// <param name="roles">Roles.</param>
    /// <returns>User has at least one of roles.</returns>
    public bool IsInRole(params UserRole[] roles)
    {
        return roles.Contains(Role);
    }

    /*/// <inheritdoc/>
    [Obsolete]
    public bool IsInRole(string role)
    {
        throw new NotImplementedException();
    }*/
}