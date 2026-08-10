using IMGR.Identity.Domain.Users;

namespace IMGR.Identity.Application.Abstractions;

public interface IIdentityStore : IAsyncDisposable
{
    Task<User?> FindUserByLoginIdAsync(string loginId, CancellationToken cancellationToken);

    Task<int> GetMaximumFailedLoginsAsync(CancellationToken cancellationToken);

    void AddLoginAudit(LoginAuditEntry entry);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}

