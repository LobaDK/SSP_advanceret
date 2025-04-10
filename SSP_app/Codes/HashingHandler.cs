using System.Security.Cryptography;
using System.Text;

namespace SSP_app.Codes;

public class HashingHandler
{
    public dynamic MD5Hashing(dynamic value)
    {
        return value is byte[]
            ? MD5.Create().ComputeHash(value)
            : Convert.ToBase64String(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(value.ToString())));
    }

    public dynamic ShaHashing(dynamic value)
    {
        return value is byte[]
        ? SHA256.Create().ComputeHash(value)
        : Convert.ToBase64String(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(value)));
    }

    public dynamic HMACHashing(dynamic value)
    {
        using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes("iougytfvhuiydtrfcgvhjuitdrfcgvhjuyt")))
        {
            return value is byte[]
                ? hmac.ComputeHash(value)
                : Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(value)));
        }
    }

    public string BCryptHashing(string value)
    {
        return BCrypt.Net.BCrypt.HashPassword(value, BCrypt.Net.BCrypt.GenerateSalt(), true, BCrypt.Net.HashType.SHA384);
    }

    public bool BCryptVeryifyHash(string value, string hashedValue)
    {
        return BCrypt.Net.BCrypt.Verify(value, hashedValue, true, BCrypt.Net.HashType.SHA384);
    }
}
