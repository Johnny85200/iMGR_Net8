namespace IMGR.Identity.Domain.Users;

public sealed class User
{
    private User()
    {
    }

    public int UserId { get; private set; }

    public string LoginId { get; private set; } = string.Empty;

    public string UserName { get; private set; } = string.Empty;

    public string? UserEmail { get; private set; }

    public string? UserMobileNo { get; private set; }

    public string PasswordHash { get; private set; } = string.Empty;

    public string AccountStatus { get; private set; } = UserAccountStatus.Inactive;

    public DateTime? ExpiryDate { get; private set; }

    public bool? UserChangePassword { get; private set; }

    public int? UserChangePasswordPeriod { get; private set; }

    public string? UserChangePasswordUnit { get; private set; }

    public DateTime? UserChangePasswordDate { get; private set; }

    public int? FailCount { get; private set; }

    public string? UserLanguage { get; private set; }

    public bool? UserIsKeepConnected { get; private set; }

    public bool? UsersCannotCreateUsersWithMorePermission { get; private set; }

    public bool IsActive => string.Equals(AccountStatus, UserAccountStatus.Active, StringComparison.OrdinalIgnoreCase);

    public static User CreateForAuthentication(
        int userId,
        string loginId,
        string userName,
        string passwordHash,
        string accountStatus = UserAccountStatus.Active,
        int failCount = 0,
        bool forcePasswordChange = false,
        int? passwordChangePeriod = null,
        string? passwordChangeUnit = null,
        DateTime? passwordChangeDate = null,
        string? language = null,
        string? userEmail = null,
        string? userMobileNo = null,
        DateTime? expiryDate = null,
        bool? userIsKeepConnected = null,
        bool? usersCannotCreateUsersWithMorePermission = null)
    {
        return new User
        {
            UserId = userId,
            LoginId = loginId,
            UserName = userName,
            PasswordHash = passwordHash,
            AccountStatus = accountStatus,
            FailCount = failCount,
            UserChangePassword = forcePasswordChange,
            UserChangePasswordPeriod = passwordChangePeriod,
            UserChangePasswordUnit = passwordChangeUnit,
            UserChangePasswordDate = passwordChangeDate,
            UserLanguage = language,
            UserEmail = userEmail,
            UserMobileNo = userMobileNo,
            ExpiryDate = expiryDate,
            UserIsKeepConnected = userIsKeepConnected,
            UsersCannotCreateUsersWithMorePermission = usersCannotCreateUsersWithMorePermission
        };
    }

    public bool RegisterFailedLogin(int maximumFailedLogins)
    {
        FailCount = (FailCount ?? 0) + 1;

        if (maximumFailedLogins <= 0 || FailCount < maximumFailedLogins)
        {
            return false;
        }

        AccountStatus = UserAccountStatus.Inactive;
        FailCount = 0;
        return true;
    }

    public void RegisterSuccessfulLogin()
    {
        FailCount = 0;
    }

    public void UpgradePasswordHash(string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        PasswordHash = passwordHash;
    }

    public bool MustChangePassword(DateTime now)
    {
        if (UserChangePassword is true)
        {
            return true;
        }

        if (UserChangePasswordDate is null || UserChangePasswordPeriod is null or <= 0)
        {
            return false;
        }

        var deadline = UserChangePasswordUnit?.ToUpperInvariant() switch
        {
            "D" => UserChangePasswordDate.Value.AddDays(UserChangePasswordPeriod.Value),
            "M" => UserChangePasswordDate.Value.AddMonths(UserChangePasswordPeriod.Value),
            "Y" => UserChangePasswordDate.Value.AddYears(UserChangePasswordPeriod.Value),
            _ => (DateTime?)null
        };

        return deadline is not null && now >= deadline.Value;
    }
}
