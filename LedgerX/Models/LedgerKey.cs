namespace LedgerX.Models;

/// <summary>
/// Represents encryption keys for secure ledger operations
/// </summary>
public class LedgerKey
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string KeyName { get; set; } = string.Empty;
    public string EncryptionKey { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
}
