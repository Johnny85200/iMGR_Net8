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

        var dbOptions = new DbContextOptionsBuilder<ControlDbContext>()
            .UseSqlServer(options.ControlDatabase.ConnectionString, sql => sql.EnableRetryOnFailure())
            .Options;
        await using var dbContext = new ControlDbContext(dbOptions);
        var company = await dbContext.CompanyDatabases
            .AsNoTracking()
            .Where(record => record.ClientCode == normalized)
            .OrderBy(record => record.CompanyDatabaseId)
            .FirstOrDefaultAsync(cancellationToken);

        if (company is null || company.IsActive is not true)
        {
            return null;
        }

        if (options.ControlDatabase.RequireImgrEnabled && company.HasImgr is not true)
        {
            return null;
        }

        if (company.DatabaseServerId is null)
        {
            throw new InvalidOperationException($"Tenant '{normalized}' has no DatabaseServer reference.");
        }

        var server = await dbContext.DatabaseServers
            .AsNoTracking()
            .SingleOrDefaultAsync(
                record => record.DatabaseServerId == company.DatabaseServerId,
                cancellationToken)
            ?? throw new InvalidOperationException(
                $"DatabaseServer {company.DatabaseServerId} for tenant '{normalized}' was not found.");

        var protector = new LegacyFieldProtector(options.ControlDatabase.LegacyEncryptionKey);
        var databaseType = protector.Unprotect(server.DatabaseType);
        if (!string.Equals(databaseType, "MSSQL", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Only MSSQL tenant databases are supported.");
        }

        var dataSource = RequireValue(protector.Unprotect(server.Location), "DatabaseServer.DBServerLocation");
        var databaseName = protector.Unprotect(company.DatabaseSchemaName);
        if (string.IsNullOrWhiteSpace(databaseName))
        {
            databaseName = RequireValue(company.ClientCode, "CompanyDatabase.CompanyDBClientCode");
        }

        var userId = protector.Unprotect(server.UserId);
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
            builder.Password = protector.Unprotect(server.Password) ?? string.Empty;
        }

        return new TenantDescriptor(normalized, builder.ConnectionString);
    }

    private static string RequireValue(string? value, string fieldName) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new InvalidOperationException($"{fieldName} is empty in the control database.")
            : value;
}

internal sealed class ControlDbContext(DbContextOptions<ControlDbContext> options) : DbContext(options)
{
    public DbSet<ControlCompanyDatabaseRecord> CompanyDatabases => Set<ControlCompanyDatabaseRecord>();

    public DbSet<ControlDatabaseServerRecord> DatabaseServers => Set<ControlDatabaseServerRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var company = modelBuilder.Entity<ControlCompanyDatabaseRecord>();
        company.ToTable("CompanyDatabase", "dbo");
        company.HasKey(record => record.CompanyDatabaseId);
        company.Property(record => record.CompanyDatabaseId).HasColumnName("CompanyDBID");
        company.Property(record => record.ClientCode).HasColumnName("CompanyDBClientCode").HasMaxLength(255);
        company.Property(record => record.DatabaseServerId).HasColumnName("DBServerID");
        company.Property(record => record.DatabaseSchemaName).HasColumnName("CompanyDBSchemaName").HasMaxLength(255);
        company.Property(record => record.IsActive).HasColumnName("CompanyDBIsActive");
        company.Property(record => record.HasImgr).HasColumnName("CompanyDBHasIMGR");

        var server = modelBuilder.Entity<ControlDatabaseServerRecord>();
        server.ToTable("DatabaseServer", "dbo");
        server.HasKey(record => record.DatabaseServerId);
        server.Property(record => record.DatabaseServerId).HasColumnName("DBServerID");
        server.Property(record => record.DatabaseType).HasColumnName("DBServerDBType").HasMaxLength(100);
        server.Property(record => record.Location).HasColumnName("DBServerLocation").HasMaxLength(255);
        server.Property(record => record.UserId).HasColumnName("DBServerUserID").HasMaxLength(255);
        server.Property(record => record.Password).HasColumnName("DBServerPassword").HasMaxLength(255);
    }
}

internal sealed class ControlCompanyDatabaseRecord
{
    public int CompanyDatabaseId { get; set; }
    public string? ClientCode { get; set; }
    public int? DatabaseServerId { get; set; }
    public string? DatabaseSchemaName { get; set; }
    public bool? IsActive { get; set; }
    public bool? HasImgr { get; set; }
}

internal sealed class ControlDatabaseServerRecord
{
    public int DatabaseServerId { get; set; }
    public string? DatabaseType { get; set; }
    public string? Location { get; set; }
    public string? UserId { get; set; }
    public string? Password { get; set; }
}
