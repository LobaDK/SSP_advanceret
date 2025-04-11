using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using WebApplication1.DTO;

namespace SSP_app.Codes
{
    public class AsymmetricEncryptionHandler
    {
        private readonly RSA _rsa;
        private const string PublicKeyFile = "publicKey.txt";
        private const string PrivateKeyFile = "privateKey.txt";
        private byte[] PublicKey;

        public AsymmetricEncryptionHandler()
        {
            _rsa = RSA.Create();
            InitializeKeys();
        }

        private void InitializeKeys()
        {
            if (File.Exists(PublicKeyFile) && File.Exists(PrivateKeyFile))
            {
                string publicKey = File.ReadAllText(PublicKeyFile);
                string privateKey = File.ReadAllText(PrivateKeyFile);

                PublicKey = Convert.FromBase64String(publicKey);

                _rsa.ImportRSAPublicKey(PublicKey, out _);
                _rsa.ImportRSAPrivateKey(Convert.FromBase64String(privateKey), out _);
            }
            else
            {
                GenerateKeys(out string publicKey, out string privateKey);
                File.WriteAllText(PublicKeyFile, publicKey);
                File.WriteAllText(PrivateKeyFile, privateKey);
            }
        }

        private void GenerateKeys(out string publicKey, out string privateKey)
        {
            publicKey = Convert.ToBase64String(_rsa.ExportRSAPublicKey());
            privateKey = Convert.ToBase64String(_rsa.ExportRSAPrivateKey());
        }

        public async Task<string> Encrypt(string data)
        {
            HttpClient httpClient = new();

            EncryptionRequest requestData = new() {
                Text = data,
                PublicKey = PublicKey
            };

            string requestJson = JsonSerializer.Serialize(requestData);

            StringContent stringContent = new(requestJson, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await httpClient.PostAsync("https://localhost:5133/api/Encryption/Encrypt", stringContent);
            
            if (response.IsSuccessStatusCode)
            {
                string encryptedData = await response.Content.ReadAsStringAsync();
                return encryptedData;
            }
            else
            {
                throw new Exception("Encryption failed.");
            }
            // var encryptedData = _rsa.Encrypt(Encoding.UTF8.GetBytes(data), RSAEncryptionPadding.Pkcs1);
            // return Convert.ToBase64String(encryptedData);
        }

        public string Decrypt(string encryptedData)
        {
            var decryptedData = _rsa.Decrypt(Convert.FromBase64String(encryptedData), RSAEncryptionPadding.Pkcs1);
            return Encoding.UTF8.GetString(decryptedData);
        }
    }
}
