namespace IMGR.LegacyData.Tenant.Entities;

public partial class SystemParameter
{
    public string ParameterCode { get; set; } = string.Empty;

    public string? ParameterDescription { get; set; }

    public string? ParameterValue { get; set; }
}

