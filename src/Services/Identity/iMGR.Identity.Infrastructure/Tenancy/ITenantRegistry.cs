namespace IMGR.Identity.Infrastructure.Tenancy;

internal interface ITenantRegistry
{
    ValueTask<TenantDescriptor?> ResolveAsync(string tenantCode, CancellationToken cancellationToken);
}
