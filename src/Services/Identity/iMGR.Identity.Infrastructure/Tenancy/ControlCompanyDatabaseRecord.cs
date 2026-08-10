namespace IMGR.Identity.Infrastructure.Tenancy;

internal sealed class ControlCompanyDatabaseRecord
{
    public int CompanyDatabaseId { get; set; }

    public string? ClientCode { get; set; }

    public int? DatabaseServerId { get; set; }

    public string? DatabaseSchemaName { get; set; }

    public bool? IsActive { get; set; }

    public bool? HasImgr { get; set; }
}
