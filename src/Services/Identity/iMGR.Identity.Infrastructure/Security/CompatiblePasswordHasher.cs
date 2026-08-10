using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using IMGR.Identity.Application.Abstractions;

namespace IMGR.Identity.Infrastructure.Security;

public sealed class CompatiblePasswordHasher : IPasswordHasher
{
    private const string Algorithm = "PBKDF2-SHA256";
    private const int Iterations = 210_000;
    private const int SaltSize = 16;
    private const int HashSize = 32;

    public string Hash(string password)
    {
        ArgumentNullException.ThrowIfNull(password);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            HashSize);

        return $"${Algorithm}${Iterations.ToString(CultureInfo.InvariantCulture)}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public PasswordVerificationResult Verify(string password, string storedHash)
    {
        ArgumentNullException.ThrowIfNull(password);

        if (string.IsNullOrWhiteSpace(storedHash))
        {
            return PasswordVerificationResult.Failed;
        }

        return storedHash.StartsWith($"${Algorithm}$", StringComparison.Ordinal)
            ? VerifyPbkdf2(password, storedHash)
            : VerifyLegacySha1(password, storedHash);
    }

    private static PasswordVerificationResult VerifyPbkdf2(string password, string storedHash)
    {
        try
        {
            var parts = storedHash.Split('$', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 4 || !string.Equals(parts[0], Algorithm, StringComparison.Ordinal))
            {
                return PasswordVerificationResult.Failed;
            }

            if (!int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var iterations) || iterations <= 0)
            {
                return PasswordVerificationResult.Failed;
            }

            var salt = Convert.FromBase64String(parts[2]);
            var expected = Convert.FromBase64String(parts[3]);
            var actual = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                expected.Length);

            if (!CryptographicOperations.FixedTimeEquals(actual, expected))
            {
                return PasswordVerificationResult.Failed;
            }

            return iterations < Iterations
                ? PasswordVerificationResult.SuccessRehashNeeded
                : PasswordVerificationResult.Success;
        }
        catch (FormatException)
        {
            return PasswordVerificationResult.Failed;
        }
    }

    private static PasswordVerificationResult VerifyLegacySha1(string password, string storedHash)
    {
        byte[] expected;
        try
        {
            expected = Convert.FromBase64String(storedHash);
        }
        catch (FormatException)
        {
            return PasswordVerificationResult.Failed;
        }

        var actual = SHA1.HashData(Encoding.Unicode.GetBytes(password));
        return CryptographicOperations.FixedTimeEquals(actual, expected)
            ? PasswordVerificationResult.SuccessRehashNeeded
            : PasswordVerificationResult.Failed;
    }
}

