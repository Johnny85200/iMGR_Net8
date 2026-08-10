namespace IMGR.Identity.Application.Authentication;

public enum LoginStatus
{
    Success,
    InvalidCredentials,
    AccountLocked,
    TenantNotConfigured
}

