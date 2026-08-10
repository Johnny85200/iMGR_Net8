using Microsoft.EntityFrameworkCore;

namespace IMGR.Identity.Infrastructure.Tenancy;

internal sealed class ControlDatabaseTenantRegistry : ITenantRegistry
{
    private readonly ControlDatabaseOptions _options;
    private readonly TenantConnectionStringFactory _connectionStringFactory;
    private readonly DbContextOptions<ControlDbContext> _dbContextOptions;

    public ControlDatabaseTenantRegistry(ControlDatabaseOptions options)
    {
        _options = options;
        var decryptor = new LegacyFieldDecryptor(options.LegacyEncryptionKey);
        _connectionStringFactory = new TenantConnectionStringFactory(options, decryptor);
        _dbContextOptions = new DbContextOptionsBuilder<ControlDbContext>()
            .UseSqlServer(options.ConnectionString, sqlServer => sqlServer.EnableRetryOnFailure())
            .Options;
    }

    public async ValueTask<TenantDescriptor?> ResolveAsync(
        string tenantCode,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(tenantCode))
        {
            return null;
        }

        var normalizedTenantCode = tenantCode.Trim().ToUpperInvariant();
        await using var dbContext = new ControlDbContext(_dbContextOptions);

        var company = await dbContext.CompanyDatabases
            .AsNoTracking()
            .Where(record => record.ClientCode == normalizedTenantCode)
            .OrderBy(record => record.CompanyDatabaseId)
            .FirstOrDefaultAsync(cancellationToken);

        if (company is null || !IsEnabled(company.IsActive))
        {
            return null;
        }

        if (_options.RequireImgrEnabled && !IsEnabled(company.HasImgr))
        {
            return null;
        }

        if (company.DatabaseServerId is null)
        {
            throw new InvalidOperationException(
                $"Tenant '{normalizedTenantCode}' does not reference a DatabaseServer record.");
        }

        var server = await dbContext.DatabaseServers
            .AsNoTracking()
            .SingleOrDefaultAsync(
                record => record.DatabaseServerId == company.DatabaseServerId.Value,
                cancellationToken)
            ?? throw new InvalidOperationException(
                $"DatabaseServer {company.DatabaseServerId.Value} for tenant '{normalizedTenantCode}' was not found.");

        var connectionString = _connectionStringFactory.Create(company, server);
        return new TenantDescriptor(normalizedTenantCode, connectionString);
    }

    private static bool IsEnabled(bool? value) => value is true;
}
