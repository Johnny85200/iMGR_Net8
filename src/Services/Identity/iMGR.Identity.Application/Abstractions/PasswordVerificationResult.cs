namespace IMGR.Identity.Application.Abstractions;

public enum PasswordVerificationResult
{
    Failed,
    Success,
    SuccessRehashNeeded
}

