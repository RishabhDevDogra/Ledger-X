using Microsoft.AspNetCore.Mvc;
using LedgerX.Models;

namespace LedgerX.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class LedgerKeysController : ControllerBase
{
    private static List<LedgerKey> _ledgerKeys = new()
    {
        new LedgerKey
        {
            KeyName = "Production Key",
            EncryptionKey = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32)),
            CreatedAt = new DateTime(2024, 1, 1),
            ExpiresAt = new DateTime(2025, 12, 31),
            IsActive = true
        },
        new LedgerKey
        {
            KeyName = "Development Key",
            EncryptionKey = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32)),
            CreatedAt = DateTime.UtcNow.AddMonths(-3),
            ExpiresAt = null,
            IsActive = true
        },
        new LedgerKey
        {
            KeyName = "Backup Key",
            EncryptionKey = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32)),
            CreatedAt = new DateTime(2023, 6, 1),
            ExpiresAt = new DateTime(2024, 6, 1),
            IsActive = false
        }
    };

    /// <summary>
    /// Get all ledger keys
    /// </summary>
    /// <returns>List of all ledger keys</returns>
    [HttpGet]
    public ActionResult<IEnumerable<LedgerKey>> GetAllKeys()
    {
        return Ok(_ledgerKeys);
    }

    /// <summary>
    /// Get ledger key by ID
    /// </summary>
    /// <param name="id">Ledger key ID</param>
    /// <returns>Ledger key details (without sensitive data)</returns>
    [HttpGet("{id}")]
    public ActionResult<LedgerKeyResponse> GetKeyById(string id)
    {
        var key = _ledgerKeys.FirstOrDefault(k => k.Id == id);
        if (key == null)
            return NotFound(new { message = "Ledger key not found" });

        return Ok(new LedgerKeyResponse
        {
            Id = key.Id,
            KeyName = key.KeyName,
            CreatedAt = key.CreatedAt,
            ExpiresAt = key.ExpiresAt,
            IsActive = key.IsActive
        });
    }

    /// <summary>
    /// Create a new ledger encryption key
    /// </summary>
    /// <param name="request">Key creation request</param>
    /// <returns>Created ledger key</returns>
    [HttpPost]
    public ActionResult<LedgerKeyResponse> CreateKey([FromBody] CreateLedgerKeyRequest request)
    {
        if (string.IsNullOrEmpty(request.KeyName))
            return BadRequest(new { message = "KeyName is required" });

        var key = new LedgerKey
        {
            KeyName = request.KeyName,
            EncryptionKey = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32)),
            ExpiresAt = request.ExpiresAt
        };

        _ledgerKeys.Add(key);
        
        return CreatedAtAction(nameof(GetKeyById), new { id = key.Id }, new LedgerKeyResponse
        {
            Id = key.Id,
            KeyName = key.KeyName,
            CreatedAt = key.CreatedAt,
            ExpiresAt = key.ExpiresAt,
            IsActive = key.IsActive
        });
    }

    /// <summary>
    /// Rotate/refresh a ledger key
    /// </summary>
    /// <param name="id">Ledger key ID</param>
    [HttpPost("{id}/rotate")]
    public ActionResult<LedgerKeyResponse> RotateKey(string id)
    {
        var key = _ledgerKeys.FirstOrDefault(k => k.Id == id);
        if (key == null)
            return NotFound(new { message = "Ledger key not found" });

        key.EncryptionKey = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
        
        return Ok(new LedgerKeyResponse
        {
            Id = key.Id,
            KeyName = key.KeyName,
            CreatedAt = key.CreatedAt,
            ExpiresAt = key.ExpiresAt,
            IsActive = key.IsActive
        });
    }

    /// <summary>
    /// Deactivate a ledger key
    /// </summary>
    /// <param name="id">Ledger key ID</param>
    [HttpPost("{id}/deactivate")]
    public ActionResult<LedgerKeyResponse> DeactivateKey(string id)
    {
        var key = _ledgerKeys.FirstOrDefault(k => k.Id == id);
        if (key == null)
            return NotFound(new { message = "Ledger key not found" });

        key.IsActive = false;
        
        return Ok(new LedgerKeyResponse
        {
            Id = key.Id,
            KeyName = key.KeyName,
            CreatedAt = key.CreatedAt,
            ExpiresAt = key.ExpiresAt,
            IsActive = key.IsActive
        });
    }

    /// <summary>
    /// Delete a ledger key
    /// </summary>
    /// <param name="id">Ledger key ID</param>
    [HttpDelete("{id}")]
    public ActionResult DeleteKey(string id)
    {
        var key = _ledgerKeys.FirstOrDefault(k => k.Id == id);
        if (key == null)
            return NotFound(new { message = "Ledger key not found" });

        _ledgerKeys.Remove(key);
        return NoContent();
    }
}

/// <summary>
/// Request model for creating a ledger key
/// </summary>
public class CreateLedgerKeyRequest
{
    public string KeyName { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
}

/// <summary>
/// Response model for ledger key (without sensitive encryption key)
/// </summary>
public class LedgerKeyResponse
{
    public string Id { get; set; } = string.Empty;
    public string KeyName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; }
}
