namespace IMGR.LegacyData.Control.Entities;

public partial class CompanyDatabase
{
    public int CompanyDatabaseId { get; set; }
    public string? ClientCode { get; set; }
    public int? DatabaseServerId { get; set; }
    public string? DatabaseSchemaName { get; set; }
    public bool? IsActive { get; set; }
    public bool? HasImgr { get; set; }
}

public partial class DatabaseServer
{
    public int DatabaseServerId { get; set; }
    public string? DatabaseType { get; set; }
    public string? Location { get; set; }
    public string? UserId { get; set; }
    public string? Password { get; set; }
}
