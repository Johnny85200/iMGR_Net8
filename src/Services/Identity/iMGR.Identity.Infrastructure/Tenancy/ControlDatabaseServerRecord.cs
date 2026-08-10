namespace IMGR.Identity.Infrastructure.Tenancy;

internal sealed class ControlDatabaseServerRecord
{
    public int DatabaseServerId { get; set; }

    public string? DatabaseType { get; set; }

    public string? Location { get; set; }

    public string? UserId { get; set; }

    public string? Password { get; set; }
}

