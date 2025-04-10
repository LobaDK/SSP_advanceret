using System.Security.Cryptography;
using System.Text;

namespace SSP_app.Codes;

public class SymmetricEncryptionHandler
{
    private readonly byte[] _key;
    private readonly byte[] _iv;
    private const string AesKeyFilePath = "aes_key.dat";

    public SymmetricEncryptionHandler()
    {
        if (File.Exists(AesKeyFilePath))
        {
            // Load AES key and IV from the file
            var aesData = File.ReadAllBytes(AesKeyFilePath);
            _key = aesData[..32]; // First 32 bytes for the AES key (256 bits)
            _iv = aesData[32..];  // Remaining bytes for the IV (16 bytes)
        }
        else
        {
            // Generate a new AES key and IV
            using var aes = Aes.Create();
            _key = aes.Key;
            _iv = aes.IV;

            // Save the AES key and IV to the file
            var aesData = new byte[_key.Length + _iv.Length];
            Array.Copy(_key, 0, aesData, 0, _key.Length);
            Array.Copy(_iv, 0, aesData, _key.Length, _iv.Length);
            File.WriteAllBytes(AesKeyFilePath, aesData);
        }
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
        {
            throw new ArgumentException("Plaintext cannot be null or empty.", nameof(plainText));
        }

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;

        var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        {
            using var sw = new StreamWriter(cs);
            sw.Write(plainText);
        }

        return Convert.ToBase64String(ms.ToArray());
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
        {
            throw new ArgumentException("Ciphertext cannot be null or empty.", nameof(cipherText));
        }

        try
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;

            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using var ms = new MemoryStream(Convert.FromBase64String(cipherText));
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);

            return sr.ReadToEnd();
        }
        catch (CryptographicException ex)
        {
            throw new CryptographicException("Decryption failed. Ensure the ciphertext, key, and IV are correct.", ex);
        }
    }
}