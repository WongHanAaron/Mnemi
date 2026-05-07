namespace Mnemi.Application.Ports;

/// <summary>
/// Service interface for encrypting and decrypting sensitive tokens.
/// </summary>
public interface ITokenEncryptionService
{
    /// <summary>
    /// Encrypts a plain text token.
    /// </summary>
    /// <param name="plainText">The plain text token to encrypt.</param>
    /// <returns>The encrypted token.</returns>
    string Encrypt(string plainText);

    /// <summary>
    /// Decrypts an encrypted token.
    /// </summary>
    /// <param name="cipherText">The encrypted token to decrypt.</param>
    /// <returns>The decrypted plain text token.</returns>
    string Decrypt(string cipherText);
}
