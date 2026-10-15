using System.Security.Cryptography;

namespace Laundry.Domain.Identity;

public enum UserRole { Admin, Customer }

public sealed class UserAccount
{
    public UserAccount(Guid id, string whatsappNumber, UserRole role, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(whatsappNumber)) throw new ArgumentException("Nomor WhatsApp wajib diisi.", nameof(whatsappNumber));
        Id = id;
        WhatsAppNumber = whatsappNumber;
        Role = role;
        PasswordHash = passwordHash;
    }

    public Guid Id { get; }
    public string WhatsAppNumber { get; }
    public UserRole Role { get; }
    public string PasswordHash { get; private set; }
    public int FailedLoginCount { get; private set; }
    public DateTimeOffset? LockedUntilUtc { get; private set; }

    public bool VerifyPassword(string password, DateTimeOffset now)
    {
        if (LockedUntilUtc.HasValue && LockedUntilUtc.Value > now) return false;
        if (PasswordHashing.Verify(password, PasswordHash))
        {
            FailedLoginCount = 0;
            LockedUntilUtc = null;
            return true;
        }

        FailedLoginCount++;
        if (FailedLoginCount >= 5) LockedUntilUtc = now.AddMinutes(15);
        return false;
    }
}

public static class PasswordHashing
{
    private const int SaltBytes = 16;
    private const int KeyBytes = 32;
    private const int Iterations = 210_000;

    public static string Hash(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8) throw new ArgumentException("Password minimal 8 karakter.", nameof(password));
        var salt = RandomNumberGenerator.GetBytes(SaltBytes);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA512, KeyBytes);
        return $"v1.{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public static bool Verify(string password, string encoded)
    {
        var parts = encoded.Split('.', 4);
        if (parts.Length != 4 || parts[0] != "v1" || !int.TryParse(parts[1], out var iterations)) return false;
        try
        {
            var salt = Convert.FromBase64String(parts[2]);
            var expected = Convert.FromBase64String(parts[3]);
            var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA512, expected.Length);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch (FormatException) { return false; }
    }
}
