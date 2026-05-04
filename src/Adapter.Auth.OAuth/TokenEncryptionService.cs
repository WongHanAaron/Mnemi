using Microsoft.AspNetCore.DataProtection;
using Mnemi.Application.Ports;

namespace Mnemi.Adapter.Auth.OAuth;

/// <summary>
/// Service for encrypting and decrypting OAuth tokens using ASP.NET Core Data Protection.
/// Uses AES-256 encryption with automatic key management.
/// </summary>
public class TokenEncryptionService : ITokenEncryptionService
{
    private readonly IDataProtector _protector;
    private const string Purpose = "Mnemi.OAuth.Tokens.v1";

    public TokenEncryptionService(IDataProtectionProvider dataProtectionProvider)
    {
        _protector = dataProtectionProvider.CreateProtector(Purpose);
    }

    /// <summary>
    /// Encrypts a plain text token.
    /// </summary>
    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            throw new ArgumentException("Plain text cannot be null or empty", nameof(plainText));

        return _protector.Protect(plainText);
    }

    /// <summary>
    /// Decrypts an encrypted token.
    /// </summary>
    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            throw new ArgumentException("Cipher text cannot be null or empty", nameof(cipherText));

        try
        {
            return _protector.Unprotect(cipherText);
        }
        catch (CryptographicException ex)
        {
            throw new InvalidOperationException("Failed to decrypt token. The token may have been tampered with or the encryption key has changed.", ex);
        }
    }
}
