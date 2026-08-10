namespace IMGR.Employee.Infrastructure.Tenancy;

public sealed class ControlDatabaseOptions
{
    public string ConnectionString { get; set; } = string.Empty;

    public string LegacyEncryptionKey { get; set; } = string.Empty;

    public bool RequireImgrEnabled { get; set; }

    public bool TenantEncrypt { get; set; } = true;

    public bool TenantTrustServerCertificate { get; set; } = true;

    public int TenantConnectTimeoutSeconds { get; set; } = 15;

    internal bool IsConfigured => !string.IsNullOrWhiteSpace(ConnectionString);

    internal void Validate()
    {
        if (TenantConnectTimeoutSeconds is < 1 or > 120)
        {
            throw new InvalidOperationException(
                "Employee:ControlDatabase:TenantConnectTimeoutSeconds must be between 1 and 120.");
        }
    }
}

public sealed class TenantDatabaseOptions
{
    public string ConnectionString { get; set; } = string.Empty;
}

internal sealed record TenantDescriptor(string Code, string ConnectionString);
