using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IMGR.Identity.Application.Abstractions;
using IMGR.Identity.Domain.Users;
using Microsoft.IdentityModel.Tokens;

namespace IMGR.Identity.Infrastructure.Security;

internal sealed class JwtTokenIssuer : ITokenIssuer
{
    private readonly JwtOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly SigningCredentials _signingCredentials;

    public JwtTokenIssuer(JwtOptions options, TimeProvider timeProvider)
    {
        _options = options;
        _timeProvider = timeProvider;
        _signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SigningKey)),
            SecurityAlgorithms.HmacSha256);
    }

    public AccessToken Issue(User user, string tenantCode, bool passwordChangeRequired)
    {
        var issuedAt = _timeProvider.GetUtcNow();
        var expiresAt = issuedAt.AddMinutes(_options.AccessTokenMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserId.ToString(System.Globalization.CultureInfo.InvariantCulture)),
            new(JwtRegisteredClaimNames.UniqueName, user.LoginId),
            new(ClaimTypes.Name, user.UserName),
            new("tenant", tenantCode),
            new("password_change_required", passwordChangeRequired ? "true" : "false"),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };

        if (!string.IsNullOrWhiteSpace(user.UserLanguage))
        {
            claims.Add(new Claim("language", user.UserLanguage));
        }

        var jwt = new JwtSecurityToken(
            _options.Issuer,
            _options.Audience,
            claims,
            issuedAt.UtcDateTime,
            expiresAt.UtcDateTime,
            _signingCredentials);

        return new AccessToken(new JwtSecurityTokenHandler().WriteToken(jwt), expiresAt);
    }
}
