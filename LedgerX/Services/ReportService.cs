using LedgerX.DTOs;
using LedgerX.Repositories;

namespace LedgerX.Services;

/// <summary>
/// Report service implementation for accounting reports
/// </summary>
public class ReportService : IReportService
{
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<ReportService> _logger;

    public ReportService(
        IJournalEntryRepository journalEntryRepository,
        IAccountRepository accountRepository,
        ILogger<ReportService> logger)
    {
        _journalEntryRepository = journalEntryRepository ?? throw new ArgumentNullException(nameof(journalEntryRepository));
        _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TrialBalanceReportDto> GetTrialBalanceAsync()
    {
        _logger.LogInformation("Generating trial balance report");

        var accounts = await _accountRepository.GetAllAsync();
        var entries = await _journalEntryRepository.GetPostedEntriesAsync();

        var balances = new Dictionary<string, (decimal Debit, decimal Credit)>();

        // Initialize with account balances
        foreach (var account in accounts)
        {
            balances[account.Code] = (0, 0);
        }

        // Calculate debits and credits from journal entries
        foreach (var entry in entries)
        {
            foreach (var line in entry.Lines)
            {
                if (!balances.ContainsKey(line.AccountCode))
                {
                    balances[line.AccountCode] = (0, 0);
                }

                var current = balances[line.AccountCode];
                balances[line.AccountCode] = (
                    current.Debit + line.DebitAmount,
                    current.Credit + line.CreditAmount
                );
            }
        }

        var trialBalanceItems = accounts
            .Where(a => balances.ContainsKey(a.Code))
            .Select(a =>
            {
                var (debit, credit) = balances[a.Code];
                return new TrialBalanceDto
                {
                    AccountCode = a.Code,
                    AccountName = a.Name,
                    Debit = debit,
                    Credit = credit
                };
            })
            .ToList();

        decimal totalDebits = trialBalanceItems.Sum(x => x.Debit);
        decimal totalCredits = trialBalanceItems.Sum(x => x.Credit);
        bool isBalanced = Math.Abs(totalDebits - totalCredits) < 0.01m;

        _logger.LogInformation(
            "Trial balance generated - Total Debits: {TotalDebits}, Total Credits: {TotalCredits}, Balanced: {IsBalanced}",
            totalDebits, totalCredits, isBalanced);

        return new TrialBalanceReportDto
        {
            Accounts = trialBalanceItems,
            TotalDebits = totalDebits,
            TotalCredits = totalCredits,
            IsBalanced = isBalanced
        };
    }

    public async Task<decimal> GetTotalDebitAsync()
    {
        var entries = await _journalEntryRepository.GetPostedEntriesAsync();
        return entries.SelectMany(e => e.Lines).Sum(l => l.DebitAmount);
    }

    public async Task<decimal> GetTotalCreditAsync()
    {
        var entries = await _journalEntryRepository.GetPostedEntriesAsync();
        return entries.SelectMany(e => e.Lines).Sum(l => l.CreditAmount);
    }
}
