using System.Globalization;
using IMGR.Identity.Application.Abstractions;
using IMGR.Database.Tenant;
using IMGR.Database.Tenant.Entities;
using Microsoft.EntityFrameworkCore;
using DatabaseUser = IMGR.Database.Tenant.Entities.User;
using DomainUser = IMGR.Identity.Domain.Users.User;
using UserAccountStatus = IMGR.Identity.Domain.Users.UserAccountStatus;

namespace IMGR.Identity.Infrastructure.Persistence;

internal sealed class EfIdentityStore(LegacyTenantDbContext dbContext) : IIdentityStore
{
    private const string MaximumFailedLoginsParameter = "LOGIN_MAX_FAIL_COUNT";
    private readonly Dictionary<int, (DomainUser Domain, DatabaseUser Entity)> trackedUsers = [];

    public async Task<DomainUser?> FindUserByLoginIdAsync(string loginId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Users
            .Where(user => user.LoginID == loginId && user.UserAccountStatus != UserAccountStatus.Deleted)
            .SingleOrDefaultAsync(cancellationToken);
        if (entity is null)
        {
            return null;
        }

        var user = DomainUser.CreateForAuthentication(
            entity.UserID,
            entity.LoginID,
            entity.UserName,
            entity.UserPassword,
            entity.UserAccountStatus,
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
            UserID = entry.UserId,
            LoginAuditLoginID = Limit(entry.LoginId, 255),
            LoginAuditLoginMachine = Limit(entry.Machine, 255),
            LoginAuditLoginIPAddress = Limit(entry.IpAddress, 255),
            LoginAuditLoginAgent = entry.UserAgent,
            LoginAuditLoginDateTime = entry.OccurredAt,
            LoginAuditIsLoginFail = entry.Failed,
            LoginAuditLoginErrorMesage = Limit(entry.ErrorMessage, 255)
        });
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        foreach (var (domain, entity) in trackedUsers.Values)
        {
            entity.UserPassword = domain.PasswordHash;
            entity.UserAccountStatus = domain.AccountStatus;
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

