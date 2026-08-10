using IMGR.Identity.Application.Abstractions;

namespace IMGR.Identity.Application.Authentication;

public sealed record LoginResult(
    LoginStatus Status,
    AccessToken? AccessToken = null,
    int? UserId = null,
    string? LoginId = null,
    string? DisplayName = null,
    string? Language = null,
    bool PasswordChangeRequired = false)
{
    public static LoginResult Failed(LoginStatus status) => new(status);
}

