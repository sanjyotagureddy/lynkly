using System.Security.Cryptography;
using System.Text;
using Lynkly.Shared.Kernel.Security.Encryption.Impl;

namespace Lynkly.Shared.Kernel.Security.Tests;

public class EncryptionServiceTests
{
    private static AesEncryptionService CreateService() => new(new EncryptionKeyManager());

    [Fact]
    public void EncryptStringAndDecrypt_ReturnsOriginalBytes()
    {
        var service = CreateService();
        const string plainText = "sensitive payload";

        byte[] encrypted = service.Encrypt(plainText);
        byte[] decrypted = service.Decrypt(encrypted);

        Assert.Equal(plainText, Encoding.UTF8.GetString(decrypted));
    }

    [Fact]
    public void EncryptBytesAndDecrypt_ReturnsOriginalBytes()
    {
        var service = CreateService();
        byte[] plainBytes = [1, 2, 3, 4, 5, 255];

        byte[] encrypted = service.Encrypt(plainBytes);
        byte[] decrypted = service.Decrypt(encrypted);

        Assert.Equal(plainBytes, decrypted);
    }

    [Fact]
    public void TenantAwareEncryption_EmbedsMetadata_ForUniformDecryption()
    {
        var service = CreateService();
        const string tenantId = "tenant-a";
        const string plainText = "tenant-aware data";

        byte[] encrypted = service.Encrypt(plainText, tenantId);
        var payload = EncryptionPayloadCodec.ReadPayload(encrypted);
        byte[] decrypted = service.Decrypt(encrypted);

        Assert.Equal(tenantId, payload.Metadata.TenantId);
        Assert.Equal(plainText, Encoding.UTF8.GetString(decrypted));
    }

    [Fact]
    public void TenantIsolation_DifferentTenantMetadataCannotDecrypt()
    {
        var service = CreateService();
        byte[] encrypted = service.Encrypt("tenant-b payload", "tenant-b");
        var parsedPayload = EncryptionPayloadCodec.ReadPayload(encrypted);

        byte[] tamperedPayload = EncryptionPayloadCodec.BuildPayload(
            parsedPayload.Metadata with { TenantId = "tenant-a" },
            parsedPayload.CipherText,
            parsedPayload.Tag);

        Assert.ThrowsAny<CryptographicException>(() => service.Decrypt(tamperedPayload));
    }

    [Fact]
    public void EncryptedPayloadTampering_ModifiedCipherTextCannotDecrypt()
    {
        var service = CreateService();
        byte[] encrypted = service.Encrypt("payload", "tenant-a");
        var parsedPayload = EncryptionPayloadCodec.ReadPayload(encrypted);
        byte[] modifiedCipherText = parsedPayload.CipherText.ToArray();
        modifiedCipherText[0] ^= 0x01;

        byte[] tamperedPayload = EncryptionPayloadCodec.BuildPayload(parsedPayload.Metadata, modifiedCipherText, parsedPayload.Tag);

        Assert.ThrowsAny<CryptographicException>(() => service.Decrypt(tamperedPayload));
    }

    [Fact]
    public void EncryptedPayloadTampering_ModifiedTagCannotDecrypt()
    {
        var service = CreateService();
        byte[] encrypted = service.Encrypt("payload", "tenant-a");
        var parsedPayload = EncryptionPayloadCodec.ReadPayload(encrypted);
        byte[] modifiedTag = parsedPayload.Tag.ToArray();
        modifiedTag[^1] ^= 0x01;

        byte[] tamperedPayload = EncryptionPayloadCodec.BuildPayload(parsedPayload.Metadata, parsedPayload.CipherText, modifiedTag);

        Assert.ThrowsAny<CryptographicException>(() => service.Decrypt(tamperedPayload));
    }

    [Fact]
    public void EncryptedPayloadTampering_ModifiedNonceCannotDecrypt()
    {
        var service = CreateService();
        byte[] encrypted = service.Encrypt("payload", "tenant-a");
        var parsedPayload = EncryptionPayloadCodec.ReadPayload(encrypted);
        byte[] modifiedNonce = parsedPayload.Metadata.Nonce.ToArray();
        modifiedNonce[0] ^= 0x01;

        byte[] tamperedPayload = EncryptionPayloadCodec.BuildPayload(
            parsedPayload.Metadata with { Nonce = modifiedNonce },
            parsedPayload.CipherText,
            parsedPayload.Tag);

        Assert.ThrowsAny<CryptographicException>(() => service.Decrypt(tamperedPayload));
    }
}
