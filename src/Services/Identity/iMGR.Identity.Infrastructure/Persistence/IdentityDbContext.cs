using IMGR.Identity.Domain.Users;
using IMGR.Identity.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace IMGR.Identity.Infrastructure.Persistence;

internal sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<SystemParameterRecord> SystemParameters => Set<SystemParameterRecord>();

    public DbSet<LoginAuditRecord> LoginAudits => Set<LoginAuditRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new SystemParameterConfiguration());
        modelBuilder.ApplyConfiguration(new LoginAuditConfiguration());
    }
}

