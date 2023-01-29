namespace CleanModels;

/// <summary>
/// Filter for check if user have role.
/// </summary>
/*[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class AllowedRolesFilter : ActionFilterAttribute
{
    public AllowedRolesFilter(IServiceProvider serviceProvider, params UserRole[] roles)
    {
        UserProvider = serviceProvider.GetRequiredService<IUserProvider>();
        Roles = roles;
    }

    public UserRole[] Roles { get; }

    private IUserProvider UserProvider { get; }

    public override void OnActionExecuting()
}*/