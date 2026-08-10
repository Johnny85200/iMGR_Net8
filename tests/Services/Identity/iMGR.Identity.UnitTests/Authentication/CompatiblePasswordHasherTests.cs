using IMGR.Identity.Application.Abstractions;
using IMGR.Identity.Infrastructure.Security;

namespace IMGR.Identity.UnitTests.Authentication;

public sealed class CompatiblePasswordHasherTests
{
    private readonly CompatiblePasswordHasher _hasher = new();

    [Fact]
    public void Verify_LegacyAdminHash_ReturnsRehashNeeded()
    {
        const string legacyAdminHash = "fIdUH9Pz71AW4S1BGQDIemBGqOg=";

        var result = _hasher.Verify("admin", legacyAdminHash);

        Assert.Equal(PasswordVerificationResult.SuccessRehashNeeded, result);
    }

    [Fact]
    public void Hash_ThenVerify_ReturnsSuccess()
    {
        var hash = _hasher.Hash("a strong password");

        var result = _hasher.Verify("a strong password", hash);

        Assert.StartsWith("$PBKDF2-SHA256$", hash, StringComparison.Ordinal);
        Assert.Equal(PasswordVerificationResult.Success, result);
    }

    [Fact]
    public void Verify_WrongPassword_ReturnsFailed()
    {
        var hash = _hasher.Hash("correct password");

        var result = _hasher.Verify("wrong password", hash);

        Assert.Equal(PasswordVerificationResult.Failed, result);
    }
}

