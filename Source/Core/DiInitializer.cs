using Microsoft.Extensions.DependencyInjection;

namespace Core;

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
    public static IServiceCollection AddCore(this IServiceCollection services)
    {
        services.AddScoped<FileConverter>();

        return services;
    }
}