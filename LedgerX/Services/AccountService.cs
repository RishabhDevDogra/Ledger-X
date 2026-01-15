using LedgerX.DTOs;
using LedgerX.Models;
using LedgerX.Repositories;

namespace LedgerX.Services;

/// <summary>
/// Account service implementation with business logic
/// </summary>
public class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<AccountService> _logger;

    public AccountService(IAccountRepository accountRepository, ILogger<AccountService> logger)
    {
        _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<AccountDto>> GetAllAccountsAsync()
    {
        _logger.LogInformation("Retrieving all accounts");
        var accounts = await _accountRepository.GetAllAsync();
        return accounts.Select(MapToDto);
    }

    public async Task<AccountDto?> GetAccountByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            _logger.LogWarning("GetAccountById called with empty id");
            return null;
        }

        var account = await _accountRepository.GetByIdAsync(id);
        if (account == null)
        {
            _logger.LogWarning("Account with id {AccountId} not found", id);
            return null;
        }

        return MapToDto(account);
    }

    public async Task<AccountDto?> GetAccountByCodeAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            _logger.LogWarning("GetAccountByCode called with empty code");
            return null;
        }

        var account = await _accountRepository.GetByCodeAsync(code);
        return account == null ? null : MapToDto(account);
    }

    public async Task<IEnumerable<AccountDto>> GetActiveAccountsAsync()
    {
        _logger.LogInformation("Retrieving active accounts");
        var accounts = await _accountRepository.GetActiveAccountsAsync();
        return accounts.Select(MapToDto);
    }

    public async Task<AccountDto> CreateAccountAsync(CreateAccountDto request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        ValidateCreateAccountRequest(request);

        // Check for duplicate code
        var existingAccount = await _accountRepository.GetByCodeAsync(request.Code);
        if (existingAccount != null)
        {
            _logger.LogWarning("Attempt to create account with duplicate code {Code}", request.Code);
            throw new InvalidOperationException($"Account with code {request.Code} already exists");
        }

        var account = new Account
        {
            Code = request.Code,
            Name = request.Name,
            Type = request.Type,
            Balance = request.Balance,
            IsActive = true
        };

        var createdAccount = await _accountRepository.AddAsync(account);
        _logger.LogInformation("Account created with id {AccountId}", createdAccount.Id);

        return MapToDto(createdAccount);
    }

    public async Task<AccountDto?> UpdateAccountAsync(string id, UpdateAccountDto request)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentNullException(nameof(id));

        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var account = await _accountRepository.GetByIdAsync(id);
        if (account == null)
        {
            _logger.LogWarning("Cannot update: Account with id {AccountId} not found", id);
            return null;
        }

        // Update only provided fields
        if (!string.IsNullOrWhiteSpace(request.Name))
            account.Name = request.Name;

        if (!string.IsNullOrWhiteSpace(request.Type))
            account.Type = request.Type;

        if (request.IsActive.HasValue)
            account.IsActive = request.IsActive.Value;

        var updatedAccount = await _accountRepository.UpdateAsync(account);
        _logger.LogInformation("Account {AccountId} updated", id);

        return MapToDto(updatedAccount);
    }

    public async Task<bool> DeleteAccountAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentNullException(nameof(id));

        var exists = await _accountRepository.ExistsAsync(id);
        if (!exists)
        {
            _logger.LogWarning("Cannot delete: Account with id {AccountId} not found", id);
            return false;
        }

        var result = await _accountRepository.DeleteAsync(id);
        if (result)
            _logger.LogInformation("Account {AccountId} deleted", id);

        return result;
    }

    private static AccountDto MapToDto(Account account)
    {
        return new AccountDto
        {
            Id = account.Id,
            Code = account.Code,
            Name = account.Name,
            Type = account.Type,
            Balance = account.Balance,
            CreatedAt = account.CreatedAt,
            IsActive = account.IsActive
        };
    }

    private static void ValidateCreateAccountRequest(CreateAccountDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
            throw new ArgumentException("Account code is required", nameof(request.Code));

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Account name is required", nameof(request.Name));

        if (string.IsNullOrWhiteSpace(request.Type))
            throw new ArgumentException("Account type is required", nameof(request.Type));

        var validTypes = new[] { "Asset", "Liability", "Equity", "Revenue", "Expense" };
        if (!validTypes.Contains(request.Type))
            throw new ArgumentException($"Invalid account type. Must be one of: {string.Join(", ", validTypes)}", nameof(request.Type));
    }
}
