using System.Security.Cryptography;
using System.Text;

namespace IMGR.Identity.Infrastructure.Tenancy;

internal sealed class LegacyFieldDecryptor(string encryptionKey)
{
    public string? Decrypt(string? value)
    {
        if (!LooksEncrypted(value, out var encryptedBytes))
        {
            return value;
        }

        try
        {
            var key = CreateLegalKey(encryptionKey);
            using var aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = key;
            aes.IV = key[..(aes.BlockSize / 8)];

            using var decryptor = aes.CreateDecryptor();
            var plaintext = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
            return Encoding.UTF8.GetString(plaintext);
        }
        catch (CryptographicException)
        {
            // The legacy transcoder treats non-decryptable Base64 values as plaintext.
            return value;
        }
    }

    private static bool LooksEncrypted(string? value, out byte[] bytes)
    {
        bytes = [];
        if (string.IsNullOrEmpty(value) || value.Length % 4 != 0 || value.Length < 20)
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

    private static byte[] CreateLegalKey(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var keySize = key.Length switch
        {
            <= 16 => 16,
            <= 24 => 24,
            _ => 32
        };

        var legalKey = key.PadRight(keySize, ' ')[..keySize];
        return Encoding.ASCII.GetBytes(legalKey);
    }
}
