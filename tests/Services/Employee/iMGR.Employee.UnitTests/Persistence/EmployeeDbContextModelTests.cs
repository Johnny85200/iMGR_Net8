using IMGR.Employee.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IMGR.Employee.UnitTests.Persistence;

public sealed class EmployeeDbContextModelTests
{
    [Fact]
    public void Model_ContainsAllLegacyEmployeeTables()
    {
        var options = new DbContextOptionsBuilder<EmployeeDbContext>()
            .UseSqlServer("Server=(local);Database=not-used;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;
        using var context = new EmployeeDbContext(options);

        var tableNames = context.Model.GetEntityTypes()
            .Select(entity => entity.GetTableName())
            .Where(name => name is not null)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.Contains("EmpPersonalInfo", tableNames);
        Assert.Contains("EmpPositionInfo", tableNames);
        Assert.Contains("EmpHierarchy", tableNames);
        Assert.Contains("EmpContractTerms", tableNames);
        Assert.Contains("EmpBankAccount", tableNames);
        Assert.Contains("EmpSpouse", tableNames);
        Assert.Contains("EmpDependant", tableNames);
        Assert.Contains("EmpEmergencyContact", tableNames);
        Assert.Contains("EmpSkill", tableNames);
        Assert.Contains("EmpQualification", tableNames);
        Assert.Contains("EmpWorkExp", tableNames);
        Assert.Contains("EmpDocument", tableNames);
    }
}
