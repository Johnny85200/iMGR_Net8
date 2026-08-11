using IMGR.LegacyData.Control;
using Microsoft.EntityFrameworkCore;

namespace IMGR.Identity.Infrastructure.Tenancy;

internal sealed class ControlDatabaseTenantRegistry : ITenantRegistry
{
    private readonly ControlDatabaseOptions _options;
    private readonly TenantConnectionStringFactory _connectionStringFactory;
    private readonly DbContextOptions<LegacyControlDbContext> _dbContextOptions;

    public ControlDatabaseTenantRegistry(ControlDatabaseOptions options)
    {
        _options = options;
        var decryptor = new LegacyFieldDecryptor(options.LegacyEncryptionKey);
        _connectionStringFactory = new TenantConnectionStringFactory(options, decryptor);
        _dbContextOptions = new DbContextOptionsBuilder<LegacyControlDbContext>()
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
        await using var dbContext = new LegacyControlDbContext(_dbContextOptions);

        var company = await dbContext.CompanyDatabases
            .AsNoTracking()
            .Where(record => record.CompanyDBClientCode == normalizedTenantCode)
            .OrderBy(record => record.CompanyDBID)
            .FirstOrDefaultAsync(cancellationToken);

        if (company is null || !IsEnabled(company.CompanyDBIsActive))
        {
            return null;
        }

        if (_options.RequireImgrEnabled && !IsEnabled(company.CompanyDBHasIMGR))
        {
            return null;
        }

        if (company.DBServerID is null)
        {
            throw new InvalidOperationException(
                $"Tenant '{normalizedTenantCode}' does not reference a DatabaseServer record.");
        }

        var server = await dbContext.DatabaseServers
            .AsNoTracking()
            .SingleOrDefaultAsync(
                record => record.DBServerID == company.DBServerID.Value,
                cancellationToken)
            ?? throw new InvalidOperationException(
                $"DatabaseServer {company.DBServerID.Value} for tenant '{normalizedTenantCode}' was not found.");

        var connectionString = _connectionStringFactory.Create(company, server);
        return new TenantDescriptor(normalizedTenantCode, connectionString);
    }

    private static bool IsEnabled(bool? value) => value is true;
}
