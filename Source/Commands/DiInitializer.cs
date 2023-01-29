using Commands.Database;
using Dal.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace Commands;

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
    public static IServiceCollection AddCommands(this IServiceCollection services)
    {
        services.AddScoped<TestConnectionCommand>();
        services.AddScoped<AddEntityCommand>();
        services.AddScoped<AddOrUpdateEntityCommand>();
        services.AddScoped<DeleteEntityCommand>();
        services.AddScoped<DeleteNonStandardEntityCommand<Passport>>();
        services.AddScoped<DeleteEmployeeIdentityEntityCommand>();
        services.AddScoped<AttachOrderToEmployeeCommand>();
        services.AddScoped<DetachOrdersFromEmployeeCommand>();
        services.AddScoped<SaveChangesEntityCommand>();
        services.AddScoped<UpdateOrderCommand>();

        return services;
    }
}