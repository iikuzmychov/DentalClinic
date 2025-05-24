using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace DentalClinic.Domain.Aggregates.UserAggregate;

public sealed record HashedPassword
{
    private const int HashLength = 64;
    private const int SaltLength = 64;
    private const int IterationsCount = 350_000;

    private static readonly HashAlgorithmName s_hashAlgorithmName = HashAlgorithmName.SHA512;

    [MinLength(HashLength)]
    [MaxLength(HashLength)]
    public byte[] Hash { get; private set; }

    [MinLength(SaltLength)]
    [MaxLength(SaltLength)]
    public byte[] Salt { get; private set; }

    public HashedPassword(byte[] hash, byte[] salt)
    {
        ArgumentNullException.ThrowIfNull(hash);
        ArgumentNullException.ThrowIfNull(salt);

        if (hash.Length != HashLength)
        {
            throw new ArgumentException("Hash length is invalid.", nameof(hash));
        }

        if (salt.Length != SaltLength)
        {
            throw new ArgumentException("Salt length is invalid.", nameof(salt));
        }

        Hash = hash;
        Salt = salt;
    }

    public static HashedPassword FromSecurePassword(SecurePassword password)
    {
        ArgumentNullException.ThrowIfNull(password);

        var salt = RandomNumberGenerator.GetBytes(HashLength);

        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password.Value),
            salt,
            IterationsCount,
            s_hashAlgorithmName,
            SaltLength);

        return new HashedPassword(hash, salt);
    }

    public bool IsMatch(string password)
    {
        ArgumentNullException.ThrowIfNull(password);

        var hashToCompare = Rfc2898DeriveBytes.Pbkdf2(
            password,
            Salt,
            IterationsCount,
            s_hashAlgorithmName,
            SaltLength);

        return CryptographicOperations.FixedTimeEquals(hashToCompare, Hash);
    }
}
