using LedgerX.Models;

namespace LedgerX.Repositories;

/// <summary>
/// In-memory repository implementation for LedgerKey entities
/// </summary>
public class LedgerKeyRepository : RepositoryBase<LedgerKey>, ILedgerKeyRepository
{
    public LedgerKeyRepository()
    {
        InitializeSampleData();
    }

    public Task<IEnumerable<LedgerKey>> GetActiveKeysAsync()
    {
        return Task.FromResult<IEnumerable<LedgerKey>>(
            _data.Where(k => k.IsActive && (k.ExpiresAt == null || k.ExpiresAt > DateTime.UtcNow)).AsEnumerable()
        );
    }

    public Task<IEnumerable<LedgerKey>> GetExpiredKeysAsync()
    {
        return Task.FromResult<IEnumerable<LedgerKey>>(
            _data.Where(k => k.ExpiresAt != null && k.ExpiresAt <= DateTime.UtcNow).AsEnumerable()
        );
    }

    protected override string GetId(LedgerKey entity)
    {
        return entity.Id;
    }

    private void InitializeSampleData()
    {
        _data.AddRange(new[]
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
        });
    }
}
