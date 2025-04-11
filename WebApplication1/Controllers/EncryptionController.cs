using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTO;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EncryptionController : ControllerBase
    {
        [HttpPost("Encrypt")]
        public async Task<ActionResult<string>> Encrypt([FromBody] EncryptionRequest request)
        {
            var _rsa = RSA.Create();
            _rsa.ImportRSAPublicKey(request.PublicKey, out _);
            var encryptedData = _rsa.Encrypt(Encoding.UTF8.GetBytes(request.Text), RSAEncryptionPadding.Pkcs1);
            return Ok(Convert.ToBase64String(encryptedData));
        }
    }
}
