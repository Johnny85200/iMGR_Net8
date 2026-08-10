using IMGR.Identity.Application.Abstractions;

namespace IMGR.Identity.Infrastructure.Persistence;

internal sealed class LoginAuditRecord
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

    public static LoginAuditRecord From(LoginAuditEntry entry)
    {
        return new LoginAuditRecord
        {
            UserId = entry.UserId,
            LoginId = Limit(entry.LoginId, 255),
            LoginMachine = Limit(entry.Machine, 255),
            LoginIpAddress = Limit(entry.IpAddress, 255),
            LoginAgent = entry.UserAgent,
            LoginDateTime = entry.OccurredAt,
            IsLoginFail = entry.Failed ? 1 : 0,
            LoginErrorMessage = Limit(entry.ErrorMessage, 255)
        };
    }

    private static string? Limit(string? value, int maximumLength)
    {
        return value is null || value.Length <= maximumLength ? value : value[..maximumLength];
    }
}

