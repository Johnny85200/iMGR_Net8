namespace IMGR.Employee.Infrastructure.Tenancy;

public sealed class ControlDatabaseOptions
{
    public string ConnectionString { get; set; } = string.Empty;

    public string LegacyEncryptionKey { get; set; } = string.Empty;

    public bool RequireImgrEnabled { get; set; }

    public bool TenantEncrypt { get; set; } = true;

    public bool TenantTrustServerCertificate { get; set; } = true;

    public int TenantConnectTimeoutSeconds { get; set; } = 15;

    public int TenantCompatibilityLevel { get; set; } = 110;

    internal bool IsConfigured => !string.IsNullOrWhiteSpace(ConnectionString);

    internal void Validate()
    {
        if (TenantConnectTimeoutSeconds is < 1 or > 120)
        {
            throw new InvalidOperationException(
                "Employee:ControlDatabase:TenantConnectTimeoutSeconds must be between 1 and 120.");
        }

        if (TenantCompatibilityLevel is < 100 or > 170)
        {
            throw new InvalidOperationException(
                "Employee:ControlDatabase:TenantCompatibilityLevel must be between 100 and 170.");
        }
    }
}

public sealed class TenantDatabaseOptions
{
    public string ConnectionString { get; set; } = string.Empty;
}

internal sealed record TenantDescriptor(string Code, string ConnectionString);
