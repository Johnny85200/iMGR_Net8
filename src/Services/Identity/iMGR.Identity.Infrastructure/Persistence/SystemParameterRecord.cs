namespace IMGR.Identity.Infrastructure.Persistence;

internal sealed class SystemParameterRecord
{
    public string ParameterCode { get; set; } = string.Empty;

    public string? ParameterDescription { get; set; }

    public string? ParameterValue { get; set; }
}

