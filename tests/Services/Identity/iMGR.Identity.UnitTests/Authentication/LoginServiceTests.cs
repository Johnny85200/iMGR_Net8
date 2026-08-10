using IMGR.Identity.Application.Abstractions;
using IMGR.Identity.Application.Authentication;
using IMGR.Identity.Domain.Users;
using IMGR.Identity.Infrastructure.Security;

namespace IMGR.Identity.UnitTests.Authentication;

public sealed class LoginServiceTests
{
    private static readonly DateTimeOffset FixedNow = new(2026, 8, 7, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task LoginAsync_WithLegacyPassword_UpgradesHashAndReturnsToken()
    {
        var user = User.CreateForAuthentication(
            1,
            "admin",
            "Administrator",
            "fIdUH9Pz71AW4S1BGQDIemBGqOg=",
            failCount: 2,
            language: "en-US");
        var store = new FakeIdentityStore(user, maximumFailedLogins: 5);
        var hasher = new CompatiblePasswordHasher();
        var service = CreateService(store, hasher);

        var result = await service.LoginAsync(CreateCommand("admin", "admin"), CancellationToken.None);

        Assert.Equal(LoginStatus.Success, result.Status);
        Assert.NotNull(result.AccessToken);
        Assert.Equal(0, user.FailCount);
        Assert.Equal(PasswordVerificationResult.Success, hasher.Verify("admin", user.PasswordHash));
        Assert.Single(store.Audits);
        Assert.False(store.Audits[0].Failed);
        Assert.Equal(1, store.SaveCount);
    }

    [Fact]
    public async Task LoginAsync_DuringLegacyCoexistence_DoesNotReplaceLegacyHash()
    {
        const string legacyHash = "fIdUH9Pz71AW4S1BGQDIemBGqOg=";
        var user = User.CreateForAuthentication(1, "admin", "Administrator", legacyHash);
        var store = new FakeIdentityStore(user, maximumFailedLogins: 5);
        var service = CreateService(store, new CompatiblePasswordHasher(), upgradeLegacyHashes: false);

        var result = await service.LoginAsync(CreateCommand("admin", "admin"), CancellationToken.None);

        Assert.Equal(LoginStatus.Success, result.Status);
        Assert.Equal(legacyHash, user.PasswordHash);
    }

    [Fact]
    public async Task LoginAsync_AtFailureLimit_LocksAccountAndWritesAudit()
    {
        var hasher = new CompatiblePasswordHasher();
        var user = User.CreateForAuthentication(
            7,
            "payroll.user",
            "Payroll User",
            hasher.Hash("correct"),
            failCount: 2);
        var store = new FakeIdentityStore(user, maximumFailedLogins: 3);
        var service = CreateService(store, hasher);

        var result = await service.LoginAsync(CreateCommand("payroll.user", "wrong"), CancellationToken.None);

        Assert.Equal(LoginStatus.AccountLocked, result.Status);
        Assert.Equal(UserAccountStatus.Inactive, user.AccountStatus);
        Assert.Equal(0, user.FailCount);
        Assert.Single(store.Audits);
        Assert.True(store.Audits[0].Failed);
        Assert.Equal(1, store.SaveCount);
    }

    [Fact]
    public async Task LoginAsync_UnknownUser_ReturnsGenericFailureAndWritesAudit()
    {
        var store = new FakeIdentityStore(user: null, maximumFailedLogins: 3);
        var service = CreateService(store, new CompatiblePasswordHasher());

        var result = await service.LoginAsync(CreateCommand("missing", "wrong"), CancellationToken.None);

        Assert.Equal(LoginStatus.InvalidCredentials, result.Status);
        Assert.Single(store.Audits);
        Assert.Equal(0, store.Audits[0].UserId);
        Assert.Equal(1, store.SaveCount);
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordPeriodElapsed_MarksTokenForPasswordChange()
    {
        var hasher = new CompatiblePasswordHasher();
        var user = User.CreateForAuthentication(
            8,
            "manager",
            "Manager",
            hasher.Hash("correct"),
            passwordChangePeriod: 30,
            passwordChangeUnit: "D",
            passwordChangeDate: FixedNow.AddDays(-31).DateTime);
        var store = new FakeIdentityStore(user, maximumFailedLogins: 3);
        var service = CreateService(store, hasher);

        var result = await service.LoginAsync(CreateCommand("manager", "correct"), CancellationToken.None);

        Assert.Equal(LoginStatus.Success, result.Status);
        Assert.True(result.PasswordChangeRequired);
    }

    private static LoginService CreateService(
        FakeIdentityStore store,
        IPasswordHasher hasher,
        bool upgradeLegacyHashes = true)
    {
        return new LoginService(
            new FakeIdentityStoreFactory(store),
            hasher,
            new FakeTokenIssuer(),
            new AuthenticationPolicy { UpgradeLegacyPasswordHashes = upgradeLegacyHashes },
            new FixedTimeProvider(FixedNow));
    }

    private static LoginCommand CreateCommand(string loginId, string password)
    {
        return new LoginCommand
        {
            TenantCode = "default",
            LoginId = loginId,
            Password = password,
            ClientIpAddress = "127.0.0.1",
            UserAgent = "unit-test"
        };
    }

    private sealed class FakeIdentityStoreFactory(FakeIdentityStore store) : IIdentityStoreFactory
    {
        public ValueTask<IIdentityStore?> OpenAsync(string tenantCode, CancellationToken cancellationToken)
        {
            return ValueTask.FromResult<IIdentityStore?>(store);
        }
    }

    private sealed class FakeIdentityStore(User? user, int maximumFailedLogins) : IIdentityStore
    {
        public List<LoginAuditEntry> Audits { get; } = [];

        public int SaveCount { get; private set; }

        public Task<User?> FindUserByLoginIdAsync(string loginId, CancellationToken cancellationToken)
        {
            return Task.FromResult(user?.LoginId == loginId ? user : null);
        }

        public Task<int> GetMaximumFailedLoginsAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(maximumFailedLogins);
        }

        public void AddLoginAudit(LoginAuditEntry entry)
        {
            Audits.Add(entry);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveCount++;
            return Task.CompletedTask;
        }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }
    }

    private sealed class FakeTokenIssuer : ITokenIssuer
    {
        public AccessToken Issue(User user, string tenantCode, bool passwordChangeRequired)
        {
            return new AccessToken("test-token", FixedNow.AddMinutes(15));
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;

        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
    }
}
