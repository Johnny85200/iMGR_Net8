using IMGR.LegacyData.Control;
using IMGR.LegacyData.Control.Entities;
using IMGR.LegacyData.Tenant;
using IMGR.LegacyData.Tenant.Entities;
using Microsoft.EntityFrameworkCore;

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

        Assert.Equal("Users", context.Model.FindEntityType(typeof(Users))?.GetTableName());
        Assert.Equal("SystemParameter", context.Model.FindEntityType(typeof(SystemParameter))?.GetTableName());
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
