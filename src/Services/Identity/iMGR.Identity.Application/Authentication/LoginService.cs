using System.Text.RegularExpressions;
using IMGR.Identity.Application.Abstractions;

namespace IMGR.Identity.Application.Authentication;

public sealed class LoginService
{
    private const string DummyLegacyHash = "fIdUH9Pz71AW4S1BGQDIemBGqOg=";
    private const string InvalidCredentialsMessage = "Invalid user name or password";

    private static readonly Regex LegacyLoginIdPattern = new(
        "^[a-zA-Z][a-zA-Z0-9_.]{0,11}$",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private readonly IIdentityStoreFactory _storeFactory;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenIssuer _tokenIssuer;
    private readonly AuthenticationPolicy _policy;
    private readonly TimeProvider _timeProvider;

    public LoginService(
        IIdentityStoreFactory storeFactory,
        IPasswordHasher passwordHasher,
        ITokenIssuer tokenIssuer,
        AuthenticationPolicy policy,
        TimeProvider timeProvider)
    {
        _storeFactory = storeFactory;
        _passwordHasher = passwordHasher;
        _tokenIssuer = tokenIssuer;
        _policy = policy;
        _timeProvider = timeProvider;
    }

    public async Task<LoginResult> LoginAsync(LoginCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        await using var store = await _storeFactory.OpenAsync(command.TenantCode, cancellationToken);
        if (store is null)
        {
            _passwordHasher.Verify(command.Password, DummyLegacyHash);
            return LoginResult.Failed(LoginStatus.TenantNotConfigured);
        }

        if (!LegacyLoginIdPattern.IsMatch(command.LoginId))
        {
            _passwordHasher.Verify(command.Password, DummyLegacyHash);
            await AuditFailureAsync(store, command, 0, InvalidCredentialsMessage, cancellationToken);
            return LoginResult.Failed(LoginStatus.InvalidCredentials);
        }

        var user = await store.FindUserByLoginIdAsync(command.LoginId, cancellationToken);
        if (user is null)
        {
            _passwordHasher.Verify(command.Password, DummyLegacyHash);
            await AuditFailureAsync(store, command, 0, InvalidCredentialsMessage, cancellationToken);
            return LoginResult.Failed(LoginStatus.InvalidCredentials);
        }

        if (!user.IsActive)
        {
            await AuditFailureAsync(store, command, user.UserId, "Account is inactive or locked", cancellationToken);
            return LoginResult.Failed(LoginStatus.AccountLocked);
        }

        var passwordResult = _passwordHasher.Verify(command.Password, user.PasswordHash);
        if (passwordResult == PasswordVerificationResult.Failed)
        {
            var maximumFailedLogins = await store.GetMaximumFailedLoginsAsync(cancellationToken);
            var locked = user.RegisterFailedLogin(maximumFailedLogins);
            var message = locked ? "Account locked after repeated login failures" : InvalidCredentialsMessage;

            await AuditFailureAsync(store, command, user.UserId, message, cancellationToken);
            return LoginResult.Failed(locked ? LoginStatus.AccountLocked : LoginStatus.InvalidCredentials);
        }

        user.RegisterSuccessfulLogin();
        if (passwordResult == PasswordVerificationResult.SuccessRehashNeeded && _policy.UpgradeLegacyPasswordHashes)
        {
            user.UpgradePasswordHash(_passwordHasher.Hash(command.Password));
        }

        var now = _timeProvider.GetLocalNow().DateTime;
        var passwordChangeRequired = user.MustChangePassword(now);
        var token = _tokenIssuer.Issue(user, command.TenantCode, passwordChangeRequired);

        store.AddLoginAudit(CreateAudit(command, user.UserId, false, null));
        await store.SaveChangesAsync(cancellationToken);

        return new LoginResult(
            LoginStatus.Success,
            token,
            user.UserId,
            user.LoginId,
            user.UserName,
            user.UserLanguage,
            passwordChangeRequired);
    }

    private async Task AuditFailureAsync(
        IIdentityStore store,
        LoginCommand command,
        int userId,
        string message,
        CancellationToken cancellationToken)
    {
        store.AddLoginAudit(CreateAudit(command, userId, true, message));
        await store.SaveChangesAsync(cancellationToken);
    }

    private LoginAuditEntry CreateAudit(LoginCommand command, int userId, bool failed, string? message)
    {
        return new LoginAuditEntry(
            userId,
            command.LoginId,
            command.ClientMachine,
            command.ClientIpAddress,
            command.UserAgent,
            _timeProvider.GetLocalNow().DateTime,
            failed,
            message);
    }
}
