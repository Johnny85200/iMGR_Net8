namespace IMGR.Identity.Api.Contracts;

public sealed record CurrentUserResponse(
    string UserId,
    string LoginId,
    string DisplayName,
    string TenantCode,
    string? Language,
    bool PasswordChangeRequired);

