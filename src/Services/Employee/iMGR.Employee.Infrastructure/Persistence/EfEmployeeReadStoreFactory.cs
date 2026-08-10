using IMGR.Employee.Application.Abstractions;
using IMGR.Employee.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace IMGR.Employee.Infrastructure.Persistence;

internal sealed class EfEmployeeReadStoreFactory(EmployeeInfrastructureOptions options)
    : IEmployeeReadStoreFactory
{
    private readonly TenantConnectionResolver _tenantResolver = new(options);

    public async ValueTask<IEmployeeReadStore?> CreateAsync(
        string tenantCode,
        CancellationToken cancellationToken)
    {
        var tenant = await _tenantResolver.ResolveAsync(tenantCode, cancellationToken);
        if (tenant is null)
        {
            return null;
        }

        var dbOptions = new DbContextOptionsBuilder<EmployeeDbContext>()
            .UseSqlServer(tenant.ConnectionString, sql => sql.EnableRetryOnFailure())
            .Options;
        return new EfEmployeeReadStore(
            new EmployeeDbContext(dbOptions),
            new LegacyFieldProtector(options.ControlDatabase.LegacyEncryptionKey));
    }
}
