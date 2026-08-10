namespace IMGR.Identity.Application.Abstractions;

public sealed record LoginAuditEntry(
    int UserId,
    string LoginId,
    string? Machine,
    string? IpAddress,
    string? UserAgent,
    DateTime OccurredAt,
    bool Failed,
    string? ErrorMessage);

