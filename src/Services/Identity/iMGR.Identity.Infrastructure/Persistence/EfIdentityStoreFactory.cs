using IMGR.Identity.Application.Abstractions;
using IMGR.Identity.Infrastructure.Tenancy;
using IMGR.LegacyData.Tenant;
using Microsoft.EntityFrameworkCore;

namespace IMGR.Identity.Infrastructure.Persistence;

internal sealed class EfIdentityStoreFactory(ITenantRegistry tenantRegistry) : IIdentityStoreFactory
{
    public async ValueTask<IIdentityStore?> OpenAsync(string tenantCode, CancellationToken cancellationToken)
    {
        var tenant = await tenantRegistry.ResolveAsync(tenantCode, cancellationToken);
        if (tenant is null)
        {
            return null;
        }

        var options = new DbContextOptionsBuilder<LegacyTenantDbContext>()
            .UseSqlServer(tenant.ConnectionString, sqlServer => sqlServer.EnableRetryOnFailure())
            .Options;

        IIdentityStore store = new EfIdentityStore(new LegacyTenantDbContext(options));
        return store;
    }
}
