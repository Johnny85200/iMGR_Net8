using IMGR.Identity.Infrastructure.Security;
using IMGR.Identity.Infrastructure.Tenancy;

namespace IMGR.Identity.Api.Configuration;

public sealed class IdentitySettings
{
    public bool UpgradeLegacyPasswordHashes { get; set; }

    public ControlDatabaseOptions ControlDatabase { get; set; } = new();

    public JwtOptions Jwt { get; set; } = new();

    public Dictionary<string, TenantDatabaseOptions> Tenants { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
