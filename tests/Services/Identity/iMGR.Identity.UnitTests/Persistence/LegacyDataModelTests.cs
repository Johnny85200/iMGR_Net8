using IMGR.Database.Control;
using IMGR.Database.Control.Entities;
using IMGR.Database.Tenant;
using IMGR.Database.Tenant.Entities;
using Microsoft.EntityFrameworkCore;
using DatabaseUser = IMGR.Database.Tenant.Entities.User;
using TenantSystemParameter = IMGR.Database.Tenant.Entities.SystemParameter;

namespace IMGR.Identity.UnitTests.Persistence;

public sealed class LegacyDataModelTests
{
    [Fact]
    public void TenantModelContainsSharedIdentityTables()
    {
        var options = new DbContextOptionsBuilder<LegacyTenantDbContext>()
            .UseSqlServer()
            .Options;
        using var context = new LegacyTenantDbContext(options);

        Assert.Equal("Users", context.Model.FindEntityType(typeof(DatabaseUser))?.GetTableName());
        Assert.Equal("SystemParameter", context.Model.FindEntityType(typeof(TenantSystemParameter))?.GetTableName());
        Assert.Equal("LoginAudit", context.Model.FindEntityType(typeof(LoginAudit))?.GetTableName());
    }

    [Fact]
    public void ControlModelContainsSharedControlTables()
    {
        var options = new DbContextOptionsBuilder<LegacyControlDbContext>()
            .UseSqlServer()
            .Options;
        using var context = new LegacyControlDbContext(options);

        Assert.Equal("CompanyDatabase", context.Model.FindEntityType(typeof(CompanyDatabase))?.GetTableName());
        Assert.Equal("DatabaseServer", context.Model.FindEntityType(typeof(DatabaseServer))?.GetTableName());
    }
}
