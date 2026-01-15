using LedgerX.Models;

namespace LedgerX.Repositories;

/// <summary>
/// In-memory repository implementation for Account entities
/// </summary>
public class AccountRepository : RepositoryBase<Account>, IAccountRepository
{
    public AccountRepository()
    {
        InitializeSampleData();
    }

    public Task<Account?> GetByCodeAsync(string code)
    {
        return Task.FromResult(_data.FirstOrDefault(a => a.Code == code));
    }

    public Task<IEnumerable<Account>> GetActiveAccountsAsync()
    {
        return Task.FromResult<IEnumerable<Account>>(_data.Where(a => a.IsActive).AsEnumerable());
    }

    protected override string GetId(Account entity)
    {
        return entity.Id;
    }

    private void InitializeSampleData()
    {
        _data.AddRange(new[]
        {
            new Account { Code = "10", Name = "Cash", Type = "Asset", Balance = 50000 },
            new Account { Code = "130", Name = "Bank Account", Type = "Asset", Balance = 100000 },
            new Account { Code = "1300", Name = "Accounts Receivable", Type = "Asset", Balance = 25000 },
            new Account { Code = "2300", Name = "Accounts Payable", Type = "Liability", Balance = 15000 },
            new Account { Code = "2100", Name = "Short Term Loans", Type = "Liability", Balance = 30000 },
            new Account { Code = "3000", Name = "Common Stock", Type = "Equity", Balance = 100000 },
            new Account { Code = "3100", Name = "Retained Earnings", Type = "Equity", Balance = 60000 },
            new Account { Code = "4000", Name = "Sales Revenue", Type = "Revenue", Balance = 200000 },
            new Account { Code = "5000", Name = "Cost of Goods Sold", Type = "Expense", Balance = 80000 },
            new Account { Code = "5100", Name = "Salary Expense", Type = "Expense", Balance = 40000 }
        });
    }
}
