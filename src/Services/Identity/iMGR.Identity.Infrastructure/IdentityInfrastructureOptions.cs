using IMGR.Identity.Infrastructure.Security;
using IMGR.Identity.Infrastructure.Tenancy;

namespace IMGR.Identity.Infrastructure;

public sealed class IdentityInfrastructureOptions
{
    public ControlDatabaseOptions ControlDatabase { get; } = new();

    public JwtOptions Jwt { get; } = new();

    public bool UpgradeLegacyPasswordHashes { get; set; }
}
