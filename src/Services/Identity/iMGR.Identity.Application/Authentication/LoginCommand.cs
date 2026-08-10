namespace IMGR.Identity.Application.Authentication;

public sealed class LoginCommand
{
    public required string TenantCode { get; init; }

    public required string LoginId { get; init; }

    public required string Password { get; init; }

    public string? ClientMachine { get; init; }

    public string? ClientIpAddress { get; init; }

    public string? UserAgent { get; init; }
}

