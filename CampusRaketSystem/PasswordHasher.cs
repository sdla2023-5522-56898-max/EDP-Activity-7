using System.Security.Cryptography;
using System.Text;

namespace CampusRaketSystem;

public static class PasswordHasher
{
    private const int SaltSize = 16;

    public static (string Hash, string Salt) CreateHash(string password)
    {
        byte[] saltBytes = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hashBytes = ComputeHash(password, saltBytes);

        return (Convert.ToBase64String(hashBytes), Convert.ToBase64String(saltBytes));
    }

    public static bool Verify(string password, string hash, string salt)
    {
        byte[] saltBytes = Convert.FromBase64String(salt);
        byte[] expectedHash = Convert.FromBase64String(hash);
        byte[] actualHash = ComputeHash(password, saltBytes);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }

    private static byte[] ComputeHash(string password, byte[] saltBytes)
    {
        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
        byte[] saltedPassword = new byte[saltBytes.Length + passwordBytes.Length];

        Buffer.BlockCopy(saltBytes, 0, saltedPassword, 0, saltBytes.Length);
        Buffer.BlockCopy(passwordBytes, 0, saltedPassword, saltBytes.Length, passwordBytes.Length);

        return SHA256.HashData(saltedPassword);
    }
}
