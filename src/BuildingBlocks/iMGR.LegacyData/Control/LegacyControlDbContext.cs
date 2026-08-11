using IMGR.LegacyData.Control.Entities;
using Microsoft.EntityFrameworkCore;

namespace IMGR.LegacyData.Control;

public partial class LegacyControlDbContext : DbContext
{
    public LegacyControlDbContext()
    {
    }

    public LegacyControlDbContext(DbContextOptions<LegacyControlDbContext> options)
        : base(options)
    {
    }

    public DbSet<CompanyDatabase> CompanyDatabases => Set<CompanyDatabase>();

    public DbSet<DatabaseServer> DatabaseServers => Set<DatabaseServer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var company = modelBuilder.Entity<CompanyDatabase>();
        company.ToTable("CompanyDatabase", "dbo");
        company.HasKey(record => record.CompanyDatabaseId).HasName("PK_CompanyDatabase");
        company.Property(record => record.CompanyDatabaseId).HasColumnName("CompanyDBID");
        company.Property(record => record.ClientCode).HasColumnName("CompanyDBClientCode").HasMaxLength(255);
        company.Property(record => record.DatabaseServerId).HasColumnName("DBServerID");
        company.Property(record => record.DatabaseSchemaName).HasColumnName("CompanyDBSchemaName").HasMaxLength(255);
        company.Property(record => record.IsActive).HasColumnName("CompanyDBIsActive");
        company.Property(record => record.HasImgr).HasColumnName("CompanyDBHasIMGR");

        var server = modelBuilder.Entity<DatabaseServer>();
        server.ToTable("DatabaseServer", "dbo");
        server.HasKey(record => record.DatabaseServerId).HasName("PK_DatabaseServer");
        server.Property(record => record.DatabaseServerId).HasColumnName("DBServerID");
        server.Property(record => record.DatabaseType).HasColumnName("DBServerDBType").HasMaxLength(100);
        server.Property(record => record.Location).HasColumnName("DBServerLocation").HasMaxLength(255);
        server.Property(record => record.UserId).HasColumnName("DBServerUserID").HasMaxLength(255);
        server.Property(record => record.Password).HasColumnName("DBServerPassword").HasMaxLength(255);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer();
        }
    }
}
