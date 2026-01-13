using Microsoft.AspNetCore.Mvc;
using LedgerX.Models;

namespace LedgerX.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ReportsController : ControllerBase
{
    private static readonly List<Account> _accounts = new();
    private static readonly List<JournalEntry> _journalEntries = new();

    /// <summary>
    /// Generate a trial balance report
    /// </summary>
    /// <returns>Trial balance showing all accounts with debit and credit totals</returns>
    [HttpGet("trial-balance")]
    public ActionResult<IEnumerable<TrialBalance>> GetTrialBalance()
    {
        // Initialize with sample data if empty
        if (_accounts.Count == 0)
        {
            _accounts.AddRange(new[]
            {
                new Account { Code = "1000", Name = "Cash", Type = "Asset", Balance = 5000 },
                new Account { Code = "2000", Name = "Accounts Payable", Type = "Liability", Balance = 1000 },
                new Account { Code = "3000", Name = "Retained Earnings", Type = "Equity", Balance = 4000 }
            });
        }

        var trialBalance = _accounts
            .Where(a => a.IsActive)
            .Select(a => new TrialBalance
            {
                AccountCode = a.Code,
                AccountName = a.Name,
                DebitTotal = a.Type == "Asset" || a.Type == "Expense" ? a.Balance : 0,
                CreditTotal = a.Type == "Liability" || a.Type == "Equity" || a.Type == "Revenue" ? a.Balance : 0
            })
            .ToList();

        return Ok(trialBalance);
    }

    /// <summary>
    /// Generate a general ledger report
    /// </summary>
    /// <param name="accountCode">Optional: filter by account code</param>
    /// <returns>General ledger details</returns>
    [HttpGet("general-ledger")]
    public ActionResult<object> GetGeneralLedger([FromQuery] string? accountCode = null)
    {
        var entries = _journalEntries.Where(e => e.IsPosted).ToList();

        if (!string.IsNullOrEmpty(accountCode))
        {
            entries = entries
                .Where(e => e.Lines.Any(l => l.AccountCode == accountCode))
                .ToList();
        }

        return Ok(new
        {
            entries,
            summary = new
            {
                totalEntries = entries.Count,
                totalDebits = entries.SelectMany(e => e.Lines).Sum(l => l.DebitAmount),
                totalCredits = entries.SelectMany(e => e.Lines).Sum(l => l.CreditAmount)
            }
        });
    }

    /// <summary>
    /// Get account balance at a specific date
    /// </summary>
    /// <param name="accountCode">Account code</param>
    /// <param name="date">Date for balance calculation</param>
    /// <returns>Account balance</returns>
    [HttpGet("account-balance")]
    public ActionResult<object> GetAccountBalance([FromQuery] string accountCode, [FromQuery] DateTime? date = null)
    {
        if (string.IsNullOrEmpty(accountCode))
            return BadRequest(new { message = "Account code is required" });

        var effectiveDate = date ?? DateTime.UtcNow;

        var balance = _journalEntries
            .Where(e => e.IsPosted && e.EntryDate <= effectiveDate)
            .SelectMany(e => e.Lines)
            .Where(l => l.AccountCode == accountCode)
            .Aggregate(0m, (sum, line) => sum + line.DebitAmount - line.CreditAmount);

        return Ok(new
        {
            accountCode,
            balance,
            asOf = effectiveDate
        });
    }
}
