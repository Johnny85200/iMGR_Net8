using IMGR.Employee.Application.Abstractions;
using IMGR.Employee.Application.Employees;
using IMGR.Employee.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace IMGR.Employee.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddEmployeeService(
        this IServiceCollection services,
        Action<EmployeeInfrastructureOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        var options = new EmployeeInfrastructureOptions();
        configure(options);
        options.Validate();

        services.AddSingleton(options);
        services.AddSingleton<IEmployeeReadStoreFactory, EfEmployeeReadStoreFactory>();
        services.AddScoped<EmployeeDirectoryService>();
        return services;
    }
}
