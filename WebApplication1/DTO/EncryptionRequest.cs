namespace WebApplication1.DTO;

public record class EncryptionRequest
{
    public required string Text { get; set; }
    public required byte[] PublicKey { get; set; }
}
