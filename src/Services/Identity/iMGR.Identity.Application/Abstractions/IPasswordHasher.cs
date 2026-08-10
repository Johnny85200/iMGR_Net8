namespace IMGR.Identity.Application.Abstractions;

public interface IPasswordHasher
{
    string Hash(string password);

    PasswordVerificationResult Verify(string password, string storedHash);
}

