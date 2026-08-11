using System.Globalization;
using IMGR.Identity.Application.Abstractions;
using IMGR.Identity.Domain.Users;
using IMGR.LegacyData.Tenant;
using IMGR.LegacyData.Tenant.Entities;
using Microsoft.EntityFrameworkCore;

namespace IMGR.Identity.Infrastructure.Persistence;

internal sealed class EfIdentityStore(LegacyTenantDbContext dbContext) : IIdentityStore
{
    private const string MaximumFailedLoginsParameter = "LOGIN_MAX_FAIL_COUNT";
    private readonly Dictionary<int, (User Domain, Users Entity)> trackedUsers = [];

    public async Task<User?> FindUserByLoginIdAsync(string loginId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Users
            .Where(user => user.LoginId == loginId && user.AccountStatus != UserAccountStatus.Deleted)
            .SingleOrDefaultAsync(cancellationToken);
        if (entity is null)
        {
            return null;
        }

        var user = User.CreateForAuthentication(
            entity.UserId,
            entity.LoginId,
            entity.UserName,
            entity.PasswordHash,
            entity.AccountStatus,
            entity.FailCount ?? 0,
            entity.UserChangePassword ?? false,
            entity.UserChangePasswordPeriod,
            entity.UserChangePasswordUnit,
            entity.UserChangePasswordDate,
            entity.UserLanguage,
            entity.UserEmail,
            entity.UserMobileNo,
            entity.ExpiryDate,
            entity.UserIsKeepConnected,
            entity.UsersCannotCreateUsersWithMorePermission);
        trackedUsers[user.UserId] = (user, entity);
        return user;
    }

    public async Task<int> GetMaximumFailedLoginsAsync(CancellationToken cancellationToken)
    {
        var value = await dbContext.SystemParameters
            .AsNoTracking()
            .Where(parameter => parameter.ParameterCode == MaximumFailedLoginsParameter)
            .Select(parameter => parameter.ParameterValue)
            .SingleOrDefaultAsync(cancellationToken);

        return int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var maximum) && maximum > 0
            ? maximum
            : 0;
    }

    public void AddLoginAudit(LoginAuditEntry entry)
    {
        dbContext.LoginAudits.Add(new LoginAudit
        {
            UserId = entry.UserId,
            LoginId = Limit(entry.LoginId, 255),
            LoginMachine = Limit(entry.Machine, 255),
            LoginIpAddress = Limit(entry.IpAddress, 255),
            LoginAgent = entry.UserAgent,
            LoginDateTime = entry.OccurredAt,
            IsLoginFail = entry.Failed ? 1 : 0,
            LoginErrorMessage = Limit(entry.ErrorMessage, 255)
        });
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        foreach (var (domain, entity) in trackedUsers.Values)
        {
            entity.PasswordHash = domain.PasswordHash;
            entity.AccountStatus = domain.AccountStatus;
            entity.FailCount = domain.FailCount;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public ValueTask DisposeAsync()
    {
        return dbContext.DisposeAsync();
    }

    private static string? Limit(string? value, int maximumLength)
    {
        return value is null || value.Length <= maximumLength ? value : value[..maximumLength];
    }
}

