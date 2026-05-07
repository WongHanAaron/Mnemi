using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;
using Mnemi.Application.Ports;

namespace Mnemi.Ui.Web.Services;

/// <summary>
/// Web-specific implementation of ITokenEncryptionService using ASP.NET Core Data Protection.
/// Provides AES-256 encryption for OAuth tokens at rest.
/// </summary>
public class WebTokenEncryptionService : ITokenEncryptionService
{
    private readonly IDataProtector _protector;
    private const string Purpose = "Mnemi.OAuth.Tokens.v1";

    public WebTokenEncryptionService(IDataProtectionProvider dataProtectionProvider)
    {
        _protector = dataProtectionProvider.CreateProtector(Purpose);
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            throw new ArgumentException("Plain text cannot be null or empty", nameof(plainText));

        return _protector.Protect(plainText);
    }

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
            throw new InvalidOperationException(
                "Failed to decrypt token. The token may have been tampered with or the encryption key has changed.",
                ex);
        }
    }
}
