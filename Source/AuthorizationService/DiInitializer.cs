using CleanModels;
using Microsoft.Extensions.DependencyInjection;

namespace AuthorizationService;

/// <summary>
/// Registrar of services in services provider.
/// </summary>
public static class DiInitializer
{
    /// <summary>
    /// Register services in services provider.
    /// </summary>
    /// <param name="services">Collection of services.</param>
    /// <returns>Changed collection of services.</returns>
    public static IServiceCollection AddAuthenticationService(this IServiceCollection services)
    {
        services.AddScoped<Authorizer>();
        services.AddScoped<AuthorizationQuery>();
        services.AddScoped<HashMachine>();
        services.AddScoped<UserPrincipal>();

        return services;
    }
}