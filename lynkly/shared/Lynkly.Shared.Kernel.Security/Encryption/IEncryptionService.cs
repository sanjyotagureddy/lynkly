namespace Lynkly.Shared.Kernel.Security.Encryption;

/// <summary>
/// Provides uniform byte-oriented encryption and decryption for tenant-agnostic and tenant-aware payloads.
/// </summary>
public interface IEncryptionService
{
    /// <summary>
    /// Encrypts UTF-8 text using the default tenant context.
    /// </summary>
    /// <param name="input">Plain text payload to encrypt.</param>
    /// <returns>Encrypted payload bytes containing encryption metadata.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is <see langword="null"/>.</exception>
    byte[] Encrypt(string input);

    /// <summary>
    /// Encrypts bytes using the default tenant context.
    /// </summary>
    /// <param name="input">Plain payload bytes to encrypt.</param>
    /// <returns>Encrypted payload bytes containing encryption metadata.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is <see langword="null"/>.</exception>
    byte[] Encrypt(byte[] input);

    /// <summary>
    /// Encrypts UTF-8 text with tenant-specific key isolation and embeds tenant metadata in the payload.
    /// </summary>
    /// <param name="input">Plain text payload to encrypt.</param>
    /// <param name="tenantId">Tenant identifier used for key derivation and metadata embedding.</param>
    /// <returns>Encrypted payload bytes containing tenant metadata.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="tenantId"/> is empty or whitespace.</exception>
    byte[] Encrypt(string input, string tenantId);

    /// <summary>
    /// Encrypts bytes with tenant-specific key isolation and embeds tenant metadata in the payload.
    /// </summary>
    /// <param name="input">Plain payload bytes to encrypt.</param>
    /// <param name="tenantId">Tenant identifier used for key derivation and metadata embedding.</param>
    /// <returns>Encrypted payload bytes containing tenant metadata.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="tenantId"/> is empty or whitespace.</exception>
    byte[] Encrypt(byte[] input, string tenantId);

    /// <summary>
    /// Decrypts payload bytes produced by this service.
    /// </summary>
    /// <param name="encryptedData">Encrypted payload bytes including metadata.</param>
    /// <returns>Decrypted payload bytes.</returns>
    /// <remarks>
    /// Decryption automatically resolves tenant context from embedded metadata.
    /// Production systems should configure a stable 32-byte base64 master key at
    /// <c>Security:Encryption:MasterKey</c> to preserve decryptability across restarts.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="encryptedData"/> is <see langword="null"/>.</exception>
    /// <exception cref="System.Security.Cryptography.CryptographicException">Thrown when payload validation or decryption fails.</exception>
    byte[] Decrypt(byte[] encryptedData);
}
