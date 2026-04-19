using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Lynkly.Shared.Kernel.Security.Encryption.Impl;

internal sealed class EncryptionKeyManager
{
    private const string MasterKeyConfigPath = "Security:Encryption:MasterKey";
    // This context is part of the key derivation contract and must remain stable for backward decryption compatibility.
    private static readonly byte[] Context = "lynkly-shared-kernel-security-encryption"u8.ToArray();
    private readonly byte[] _masterKey;

    public EncryptionKeyManager()
    {
        _masterKey = RandomNumberGenerator.GetBytes(EncryptionConstants.KeySize);
    }

    public EncryptionKeyManager(byte[] masterKey)
    {
        ArgumentNullException.ThrowIfNull(masterKey);
        if (masterKey.Length != EncryptionConstants.KeySize)
        {
            throw new ArgumentException($"Master key must be {EncryptionConstants.KeySize} bytes.", nameof(masterKey));
        }

        _masterKey = masterKey.ToArray();
    }

    public static EncryptionKeyManager Create(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        string? configuredKey = configuration[MasterKeyConfigPath];
        if (string.IsNullOrWhiteSpace(configuredKey))
        {
            return new EncryptionKeyManager();
        }

        try
        {
            byte[] masterKey = Convert.FromBase64String(configuredKey);
            return new EncryptionKeyManager(masterKey);
        }
        catch (FormatException exception)
        {
            throw new ArgumentException(
                $"Configuration value at '{MasterKeyConfigPath}' must be a valid base64 string.",
                MasterKeyConfigPath,
                exception);
        }
    }

    public byte[] DeriveKey(string tenantId, byte[] salt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);
        ArgumentNullException.ThrowIfNull(salt);

        byte[] tenantBytes = Encoding.UTF8.GetBytes(tenantId);
        byte[] derivationSalt = new byte[salt.Length + tenantBytes.Length + Context.Length];
        salt.CopyTo(derivationSalt, 0);
        tenantBytes.CopyTo(derivationSalt, salt.Length);
        Context.CopyTo(derivationSalt, salt.Length + tenantBytes.Length);

        return Rfc2898DeriveBytes.Pbkdf2(_masterKey, derivationSalt, EncryptionConstants.IterationCount, HashAlgorithmName.SHA256, EncryptionConstants.KeySize);
    }
}
