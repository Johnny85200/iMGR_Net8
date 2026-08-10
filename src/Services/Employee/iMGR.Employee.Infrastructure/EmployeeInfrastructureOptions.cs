using IMGR.Employee.Infrastructure.Tenancy;

namespace IMGR.Employee.Infrastructure;

public sealed class EmployeeInfrastructureOptions
{
    public ControlDatabaseOptions ControlDatabase { get; } = new();

    public Dictionary<string, TenantDatabaseOptions> Tenants { get; } = new(StringComparer.OrdinalIgnoreCase);

    internal void Validate()
    {
        ControlDatabase.Validate();
        if (string.IsNullOrWhiteSpace(ControlDatabase.LegacyEncryptionKey))
        {
            throw new InvalidOperationException(
                "Employee:ControlDatabase:LegacyEncryptionKey is required to read legacy encrypted fields.");
        }

        if (!ControlDatabase.IsConfigured && Tenants.Count == 0)
        {
            throw new InvalidOperationException(
                "Configure Employee:ControlDatabase:ConnectionString or at least one Employee:Tenants entry.");
        }
    }
}
