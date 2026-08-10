namespace IMGR.Identity.Infrastructure.Security;

public sealed class JwtOptions
{
    public string Issuer { get; set; } = "imgr.identity";

    public string Audience { get; set; } = "imgr.api";

    public string SigningKey { get; set; } = string.Empty;

    public int AccessTokenMinutes { get; set; } = 15;

    internal void Validate()
    {
        if (SigningKey.Length < 32)
        {
            throw new InvalidOperationException("Identity:Jwt:SigningKey must contain at least 32 characters.");
        }

        if (AccessTokenMinutes is < 1 or > 1440)
        {
            throw new InvalidOperationException("Identity:Jwt:AccessTokenMinutes must be between 1 and 1440.");
        }
    }
}

