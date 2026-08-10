using Microsoft.EntityFrameworkCore;

namespace IMGR.Identity.Infrastructure.Tenancy;

internal sealed class ControlDbContext(DbContextOptions<ControlDbContext> options) : DbContext(options)
{
    public DbSet<ControlCompanyDatabaseRecord> CompanyDatabases => Set<ControlCompanyDatabaseRecord>();

    public DbSet<ControlDatabaseServerRecord> DatabaseServers => Set<ControlDatabaseServerRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var company = modelBuilder.Entity<ControlCompanyDatabaseRecord>();
        company.ToTable("CompanyDatabase", "dbo");
        company.HasKey(record => record.CompanyDatabaseId).HasName("PK_CompanyDatabase");
        company.Property(record => record.CompanyDatabaseId).HasColumnName("CompanyDBID");
        company.Property(record => record.ClientCode).HasColumnName("CompanyDBClientCode").HasMaxLength(255);
        company.Property(record => record.DatabaseServerId).HasColumnName("DBServerID");
        company.Property(record => record.DatabaseSchemaName).HasColumnName("CompanyDBSchemaName").HasMaxLength(255);
        company.Property(record => record.IsActive).HasColumnName("CompanyDBIsActive");
        company.Property(record => record.HasImgr).HasColumnName("CompanyDBHasIMGR");

        var server = modelBuilder.Entity<ControlDatabaseServerRecord>();
        server.ToTable("DatabaseServer", "dbo");
        server.HasKey(record => record.DatabaseServerId).HasName("PK_DatabaseServer");
        server.Property(record => record.DatabaseServerId).HasColumnName("DBServerID");
        server.Property(record => record.DatabaseType).HasColumnName("DBServerDBType").HasMaxLength(100);
        server.Property(record => record.Location).HasColumnName("DBServerLocation").HasMaxLength(255);
        server.Property(record => record.UserId).HasColumnName("DBServerUserID").HasMaxLength(255);
        server.Property(record => record.Password).HasColumnName("DBServerPassword").HasMaxLength(255);
    }
}

