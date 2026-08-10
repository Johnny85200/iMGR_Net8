using System.Globalization;
using IMGR.Identity.Application.Abstractions;
using IMGR.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace IMGR.Identity.Infrastructure.Persistence;

internal sealed class EfIdentityStore(IdentityDbContext dbContext) : IIdentityStore
{
    private const string MaximumFailedLoginsParameter = "LOGIN_MAX_FAIL_COUNT";

    public Task<User?> FindUserByLoginIdAsync(string loginId, CancellationToken cancellationToken)
    {
        return dbContext.Users
            .Where(user => user.LoginId == loginId && user.AccountStatus != UserAccountStatus.Deleted)
            .SingleOrDefaultAsync(cancellationToken);
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
        dbContext.LoginAudits.Add(LoginAuditRecord.From(entry));
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public ValueTask DisposeAsync()
    {
        return dbContext.DisposeAsync();
    }
}

