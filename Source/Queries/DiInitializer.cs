using Microsoft.Extensions.DependencyInjection;

namespace Queries;

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
    public static IServiceCollection AddQueries(this IServiceCollection services)
    {
        services.AddScoped<CustomersQuery>();
        services.AddScoped<EmployeesQuery>();
        services.AddScoped<EmployeesQueryByLogin>();
        services.AddScoped<EmployeesQueryByOrderNumber>();
        services.AddScoped<OrdersQuery>();
        services.AddScoped<OrdersQueryByNumber>();
        services.AddScoped<OrdersQueryByCustomer>();
        services.AddScoped<OrdersQueryByCustomerLogin>();
        services.AddScoped<OrdersQueryByEmployeeLogin>();
        services.AddScoped<PassportsQuery>();
        services.AddScoped<StatementOfWorksQuery>();
        services.AddScoped<UsersQuery>();
        services.AddScoped<UsersQueryByLogin>();

        return services;
    }
}