using LedgerX.Models;

namespace LedgerX.Repositories;

/// <summary>
/// Repository interface for LedgerKey entity
/// </summary>
public interface ILedgerKeyRepository : IRepository<LedgerKey>
{
    Task<IEnumerable<LedgerKey>> GetActiveKeysAsync();
    Task<IEnumerable<LedgerKey>> GetExpiredKeysAsync();
}
