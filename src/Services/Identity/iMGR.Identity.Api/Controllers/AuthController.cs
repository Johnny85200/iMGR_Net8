using System.IdentityModel.Tokens.Jwt;
using IMGR.Identity.Api.Contracts;
using IMGR.Identity.Application.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace IMGR.Identity.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(LoginService loginService) : ControllerBase
{
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    [HttpPost("login")]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(423)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var command = new LoginCommand
        {
            TenantCode = request.TenantCode.Trim(),
            LoginId = request.LoginId.Trim(),
            Password = request.Password,
            ClientMachine = Request.Host.Host,
            ClientIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers.UserAgent.ToString()
        };

        var result = await loginService.LoginAsync(command, cancellationToken);
        return result.Status switch
        {
            LoginStatus.Success => Ok(LoginResponse.From(result)),
            LoginStatus.AccountLocked => Problem(
                statusCode: 423,
                title: "Account locked",
                detail: "The account is inactive or has been locked."),
            LoginStatus.InvalidCredentials or LoginStatus.TenantNotConfigured => Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Authentication failed",
                detail: "Invalid tenant, user name, or password."),
            _ => Problem(statusCode: StatusCodes.Status500InternalServerError)
        };
    }

    [Authorize]
    [HttpGet("UserInformation")]
    [ProducesResponseType<CurrentUserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<CurrentUserResponse> UserInformation()
    {
        return Ok(new CurrentUserResponse(
            User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? string.Empty,
            User.FindFirst(JwtRegisteredClaimNames.UniqueName)?.Value ?? string.Empty,
            User.Identity?.Name ?? string.Empty,
            User.FindFirst("tenant")?.Value ?? string.Empty,
            User.FindFirst("language")?.Value,
            string.Equals(User.FindFirst("password_change_required")?.Value, "true", StringComparison.OrdinalIgnoreCase)));
    }
}

