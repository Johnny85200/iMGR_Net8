using System.Security.Cryptography;
using System.Text;

namespace IMGR.Employee.Infrastructure.Tenancy;

internal sealed class LegacyFieldProtector(string encryptionKey)
{
    public string? Unprotect(string? value)
    {
        if (!LooksEncrypted(value, out var encryptedBytes))
        {
            return value;
        }

        try
        {
            var key = CreateLegalKey();
            using var aes = CreateAes(key);
            using var decryptor = aes.CreateDecryptor();
            var plaintext = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
            return Encoding.UTF8.GetString(plaintext);
        }
        catch (CryptographicException)
        {
            return value;
        }
    }

    public string Protect(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var key = CreateLegalKey();
        using var aes = CreateAes(key);
        using var encryptor = aes.CreateEncryptor();
        var plaintext = Encoding.UTF8.GetBytes(value);
        return Convert.ToBase64String(encryptor.TransformFinalBlock(plaintext, 0, plaintext.Length));
    }

    private byte[] CreateLegalKey()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(encryptionKey);
        var keySize = encryptionKey.Length switch
        {
            <= 16 => 16,
            <= 24 => 24,
            _ => 32
        };
        return Encoding.ASCII.GetBytes(encryptionKey.PadRight(keySize, ' ')[..keySize]);
    }

    private static Aes CreateAes(byte[] key)
    {
        var aes = Aes.Create();
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = key;
        aes.IV = key[..(aes.BlockSize / 8)];
        return aes;
    }

    private static bool LooksEncrypted(string? value, out byte[] bytes)
    {
        bytes = [];
        if (string.IsNullOrEmpty(value) || value.Length < 20 || value.Length % 4 != 0)
        {
            return false;
        }

        try
        {
            bytes = Convert.FromBase64String(value);
            return bytes.Length > 0 && bytes.Length % 16 == 0;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
