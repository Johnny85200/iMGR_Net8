namespace IMGR.LegacyData.Tenant.Entities;

public partial class LoginAudit
{
    public int LoginAuditId { get; set; }

    public int? UserId { get; set; }

    public string? LoginId { get; set; }

    public string? LoginMachine { get; set; }

    public string? LoginIpAddress { get; set; }

    public string? LoginAgent { get; set; }

    public DateTime? LoginDateTime { get; set; }

    public int? IsLoginFail { get; set; }

    public string? LoginErrorMessage { get; set; }

}

