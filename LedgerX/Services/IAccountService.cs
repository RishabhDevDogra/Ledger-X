using LedgerX.DTOs;

namespace LedgerX.Services;

/// <summary>
/// Service interface for Account business logic
/// </summary>
public interface IAccountService
{
    Task<IEnumerable<AccountDto>> GetAllAccountsAsync();
    Task<AccountDto?> GetAccountByIdAsync(string id);
    Task<AccountDto?> GetAccountByCodeAsync(string code);
    Task<IEnumerable<AccountDto>> GetActiveAccountsAsync();
    Task<AccountDto> CreateAccountAsync(CreateAccountDto request);
    Task<AccountDto?> UpdateAccountAsync(string id, UpdateAccountDto request);
    Task<bool> DeleteAccountAsync(string id);
}
