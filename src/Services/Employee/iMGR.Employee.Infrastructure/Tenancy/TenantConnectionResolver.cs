using IMGR.LegacyData.Control;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace IMGR.Employee.Infrastructure.Tenancy;

internal sealed class TenantConnectionResolver(EmployeeInfrastructureOptions options)
{
    public async ValueTask<TenantDescriptor?> ResolveAsync(
        string tenantCode,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(tenantCode))
        {
            return null;
        }

        var normalized = tenantCode.Trim().ToUpperInvariant();
        if (!options.ControlDatabase.IsConfigured)
        {
            return options.Tenants.TryGetValue(normalized, out var configured)
                && !string.IsNullOrWhiteSpace(configured.ConnectionString)
                    ? new TenantDescriptor(normalized, configured.ConnectionString)
                    : null;
        }

        var dbOptions = new DbContextOptionsBuilder<LegacyControlDbContext>()
            .UseSqlServer(options.ControlDatabase.ConnectionString, sql => sql.EnableRetryOnFailure())
            .Options;
        await using var dbContext = new LegacyControlDbContext(dbOptions);
        var company = await dbContext.CompanyDatabases
            .AsNoTracking()
            .Where(record => record.CompanyDBClientCode == normalized)
            .OrderBy(record => record.CompanyDBID)
            .FirstOrDefaultAsync(cancellationToken);

        if (company is null || company.CompanyDBIsActive is not true)
        {
            return null;
        }

        if (options.ControlDatabase.RequireImgrEnabled && company.CompanyDBHasIMGR is not true)
        {
            return null;
        }

        if (company.DBServerID is null)
        {
            throw new InvalidOperationException($"Tenant '{normalized}' has no DatabaseServer reference.");
        }

        var server = await dbContext.DatabaseServers
            .AsNoTracking()
            .SingleOrDefaultAsync(
                record => record.DBServerID == company.DBServerID,
                cancellationToken)
            ?? throw new InvalidOperationException(
                $"DatabaseServer {company.DBServerID} for tenant '{normalized}' was not found.");

        var protector = new LegacyFieldProtector(options.ControlDatabase.LegacyEncryptionKey);
        var databaseType = protector.Unprotect(server.DBServerDBType);
        if (!string.Equals(databaseType, "MSSQL", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Only MSSQL tenant databases are supported.");
        }

        var dataSource = RequireValue(protector.Unprotect(server.DBServerLocation), "DatabaseServer.DBServerLocation");
        var databaseName = protector.Unprotect(company.CompanyDBSchemaName);
        if (string.IsNullOrWhiteSpace(databaseName))
        {
            databaseName = RequireValue(company.CompanyDBClientCode, "CompanyDatabase.CompanyDBClientCode");
        }

        var userId = protector.Unprotect(server.DBServerUserID);
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = dataSource,
            InitialCatalog = databaseName,
            Encrypt = options.ControlDatabase.TenantEncrypt,
            TrustServerCertificate = options.ControlDatabase.TenantTrustServerCertificate,
            ConnectTimeout = options.ControlDatabase.TenantConnectTimeoutSeconds,
            PersistSecurityInfo = false,
            MultipleActiveResultSets = true
        };
        if (string.IsNullOrWhiteSpace(userId))
        {
            builder.IntegratedSecurity = true;
        }
        else
        {
            builder.UserID = userId;
            builder.Password = protector.Unprotect(server.DBServerPassword) ?? string.Empty;
        }

        return new TenantDescriptor(normalized, builder.ConnectionString);
    }

    private static string RequireValue(string? value, string fieldName) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new InvalidOperationException($"{fieldName} is empty in the control database.")
            : value;
}
