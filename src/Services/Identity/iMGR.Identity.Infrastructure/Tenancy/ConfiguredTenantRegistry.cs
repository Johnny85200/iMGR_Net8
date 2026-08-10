namespace IMGR.Identity.Infrastructure.Tenancy;

internal sealed class ConfiguredTenantRegistry : ITenantRegistry
{
    private readonly IReadOnlyDictionary<string, TenantDescriptor> _tenants;

    public ConfiguredTenantRegistry(IReadOnlyDictionary<string, TenantDatabaseOptions> tenants)
    {
        _tenants = tenants
            .Where(pair => !string.IsNullOrWhiteSpace(pair.Key) && !string.IsNullOrWhiteSpace(pair.Value.ConnectionString))
            .ToDictionary(
                pair => pair.Key.Trim(),
                pair => new TenantDescriptor(pair.Key.Trim(), pair.Value.ConnectionString),
                StringComparer.OrdinalIgnoreCase);
    }

    public ValueTask<TenantDescriptor?> ResolveAsync(string tenantCode, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(tenantCode))
        {
            return ValueTask.FromResult<TenantDescriptor?>(null);
        }

        return ValueTask.FromResult<TenantDescriptor?>(_tenants.GetValueOrDefault(tenantCode.Trim()));
    }
}
