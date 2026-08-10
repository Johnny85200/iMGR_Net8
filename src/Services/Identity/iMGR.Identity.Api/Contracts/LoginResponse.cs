using IMGR.Identity.Application.Authentication;

namespace IMGR.Identity.Api.Contracts;

public sealed record LoginResponse(
    string AccessToken,
    string TokenType,
    DateTimeOffset ExpiresAt,
    AuthenticatedUserResponse User)
{
    public static LoginResponse From(LoginResult result)
    {
        ArgumentNullException.ThrowIfNull(result.AccessToken);

        return new LoginResponse(
            result.AccessToken.Value,
            "Bearer",
            result.AccessToken.ExpiresAt,
            new AuthenticatedUserResponse(
                result.UserId!.Value,
                result.LoginId!,
                result.DisplayName!,
                result.Language,
                result.PasswordChangeRequired));
    }
}

public sealed record AuthenticatedUserResponse(
    int UserId,
    string LoginId,
    string DisplayName,
    string? Language,
    bool PasswordChangeRequired);

