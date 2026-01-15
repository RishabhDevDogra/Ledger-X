using LedgerX.Models;

namespace LedgerX.Repositories;

/// <summary>
/// Repository interface for Account entity
/// </summary>
public interface IAccountRepository : IRepository<Account>
{
    Task<Account?> GetByCodeAsync(string code);
    Task<IEnumerable<Account>> GetActiveAccountsAsync();
}
