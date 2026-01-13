using Microsoft.AspNetCore.Mvc;
using LedgerX.Models;

namespace LedgerX.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AccountsController : ControllerBase
{
    private static List<Account> _accounts = new()
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
    };

    /// <summary>
    /// Get all accounts
    /// </summary>
    /// <returns>List of all accounts</returns>
    [HttpGet]
    public ActionResult<IEnumerable<Account>> GetAllAccounts()
    {
        return Ok(_accounts);
    }

    /// <summary>
    /// Get account by ID
    /// </summary>
    /// <param name="id">Account ID</param>
    /// <returns>Account details</returns>
    [HttpGet("{id}")]
    public ActionResult<Account> GetAccountById(string id)
    {
        var account = _accounts.FirstOrDefault(a => a.Id == id);
        if (account == null)
            return NotFound(new { message = "Account not found" });

        return Ok(account);
    }

    /// <summary>
    /// Create a new account
    /// </summary>
    /// <param name="request">Account creation request</param>
    /// <returns>Created account</returns>
    [HttpPost]
    public ActionResult<Account> CreateAccount([FromBody] CreateAccountRequest request)
    {
        if (string.IsNullOrEmpty(request.Code) || string.IsNullOrEmpty(request.Name))
            return BadRequest(new { message = "Code and Name are required" });

        var account = new Account
        {
            Code = request.Code,
            Name = request.Name,
            Type = request.Type,
            Balance = request.Balance
        };

        _accounts.Add(account);
        return CreatedAtAction(nameof(GetAccountById), new { id = account.Id }, account);
    }

    /// <summary>
    /// Update an account
    /// </summary>
    /// <param name="id">Account ID</param>
    /// <param name="request">Update request</param>
    /// <returns>Updated account</returns>
    [HttpPut("{id}")]
    public ActionResult<Account> UpdateAccount(string id, [FromBody] UpdateAccountRequest request)
    {
        var account = _accounts.FirstOrDefault(a => a.Id == id);
        if (account == null)
            return NotFound(new { message = "Account not found" });

        account.Name = request.Name ?? account.Name;
        account.Type = request.Type ?? account.Type;
        account.IsActive = request.IsActive ?? account.IsActive;

        return Ok(account);
    }

    /// <summary>
    /// Delete an account
    /// </summary>
    /// <param name="id">Account ID</param>
    [HttpDelete("{id}")]
    public ActionResult DeleteAccount(string id)
    {
        var account = _accounts.FirstOrDefault(a => a.Id == id);
        if (account == null)
            return NotFound(new { message = "Account not found" });

        _accounts.Remove(account);
        return NoContent();
    }
}

/// <summary>
/// Request model for creating an account
/// </summary>
public class CreateAccountRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Asset, Liability, Equity, Revenue, Expense
    public decimal Balance { get; set; }
}

/// <summary>
/// Request model for updating an account
/// </summary>
public class UpdateAccountRequest
{
    public string? Name { get; set; }
    public string? Type { get; set; }
    public bool? IsActive { get; set; }
}
