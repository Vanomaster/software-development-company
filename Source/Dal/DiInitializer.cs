using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Dal;

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
    public static IServiceCollection AddDal(this IServiceCollection services)
    {
        services.AddDbContextFactory<Context>(GetOptions(), ServiceLifetime.Scoped);

        return services;
    }

    private static Action<DbContextOptionsBuilder> GetOptions()
    {
        var configuration = new ConfigurationHelper();

        return options => options.UseSqlServer(configuration.MainConnectionString);
    }
}