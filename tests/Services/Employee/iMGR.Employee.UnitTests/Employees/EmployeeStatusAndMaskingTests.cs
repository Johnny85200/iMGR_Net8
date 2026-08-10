using IMGR.Employee.Domain.Employees;

namespace IMGR.Employee.UnitTests.Employees;

public sealed class EmployeeStatusAndMaskingTests
{
    [Theory]
    [InlineData("A", EmployeeStatus.Active)]
    [InlineData("T", EmployeeStatus.Terminated)]
    [InlineData("P", EmployeeStatus.Pending)]
    [InlineData("other", EmployeeStatus.Unknown)]
    public void ParseLegacyStatus_ReturnsExpectedStatus(string value, EmployeeStatus expected)
    {
        Assert.Equal(expected, EmployeeStatusCodes.Parse(value));
    }

    [Fact]
    public void Mask_SensitiveValue_OnlyExposesSuffix()
    {
        Assert.Equal("******6789", SensitiveValueMasker.Mask("A123456789"));
    }
}
