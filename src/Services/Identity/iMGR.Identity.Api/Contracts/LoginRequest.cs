using System.ComponentModel.DataAnnotations;

namespace IMGR.Identity.Api.Contracts;

public sealed class LoginRequest
{
    [Required]
    [StringLength(20, MinimumLength = 1)]
    public required string TenantCode { get; init; }

    [Required]
    [StringLength(20, MinimumLength = 1)]
    public required string LoginId { get; init; }

    [Required]
    [StringLength(512, MinimumLength = 1)]
    public required string Password { get; init; }
}

