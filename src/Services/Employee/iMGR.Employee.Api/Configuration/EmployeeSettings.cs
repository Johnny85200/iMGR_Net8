using IMGR.Employee.Infrastructure.Tenancy;

namespace IMGR.Employee.Api.Configuration;

public sealed class EmployeeSettings
{
    public ControlDatabaseOptions ControlDatabase { get; set; } = new();

    public Dictionary<string, TenantDatabaseOptions> Tenants { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public sealed class AuthenticationSettings
{
    public string Issuer { get; set; } = "imgr.identity";

    public string Audience { get; set; } = "imgr.api";

    public string SigningKey { get; set; } = string.Empty;

    public void Validate()
    {
        if (SigningKey.Length < 32)
        {
            throw new InvalidOperationException(
                "Authentication:SigningKey must contain at least 32 characters.");
        }
    }
}
