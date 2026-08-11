namespace IMGR.LegacyData.Tenant.Entities;

public partial class Users
{
    public int UserId { get; set; }
    public string LoginId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? UserEmail { get; set; }
    public string? UserMobileNo { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public string AccountStatus { get; set; } = string.Empty;
    public DateTime? ExpiryDate { get; set; }
    public bool? UserChangePassword { get; set; }
    public int? UserChangePasswordPeriod { get; set; }
    public string? UserChangePasswordUnit { get; set; }
    public DateTime? UserChangePasswordDate { get; set; }
    public int? FailCount { get; set; }
    public string? UserLanguage { get; set; }
    public bool? UserIsKeepConnected { get; set; }
    public bool? UsersCannotCreateUsersWithMorePermission { get; set; }
}
