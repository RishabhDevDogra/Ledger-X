using LedgerX.Models;

namespace LedgerX.Repositories;

/// <summary>
/// In-memory repository implementation for JournalEntry entities
/// </summary>
public class JournalEntryRepository : RepositoryBase<JournalEntry>, IJournalEntryRepository
{
    public JournalEntryRepository()
    {
        InitializeSampleData();
    }

    public Task<IEnumerable<JournalEntry>> GetPostedEntriesAsync()
    {
        return Task.FromResult<IEnumerable<JournalEntry>>(_data.Where(e => e.IsPosted).AsEnumerable());
    }

    public Task<IEnumerable<JournalEntry>> GetEntriesByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return Task.FromResult<IEnumerable<JournalEntry>>(
            _data.Where(e => e.EntryDate >= startDate && e.EntryDate <= endDate).AsEnumerable()
        );
    }

    protected override string GetId(JournalEntry entity)
    {
        return entity.Id;
    }

    private void InitializeSampleData()
    {
        _data.AddRange(new[]
        {
            new JournalEntry
            {
                Description = "Initial cash deposit",
                ReferenceNumber = "JE-001",
                EntryDate = new DateTime(2024, 1, 1),
                IsPosted = true,
                Lines = new()
                {
                    new JournalLine { AccountCode = "1010", AccountId = "1", DebitAmount = 50000, Narration = "Debit Bank" },
                    new JournalLine { AccountCode = "3000", AccountId = "2", CreditAmount = 50000, Narration = "Credit Common Stock" }
                }
            },
            new JournalEntry
            {
                Description = "Sales transaction",
                ReferenceNumber = "JE-002",
                EntryDate = new DateTime(2024, 1, 5),
                IsPosted = true,
                Lines = new()
                {
                    new JournalLine { AccountCode = "1000", AccountId = "1", DebitAmount = 25000, Narration = "Debit Cash" },
                    new JournalLine { AccountCode = "4000", AccountId = "3", CreditAmount = 25000, Narration = "Credit Sales Revenue" }
                }
            },
            new JournalEntry
            {
                Description = "Purchase inventory on credit",
                ReferenceNumber = "JE-003",
                EntryDate = new DateTime(2024, 1, 10),
                IsPosted = true,
                Lines = new()
                {
                    new JournalLine { AccountCode = "5000", AccountId = "4", DebitAmount = 10000, Narration = "Debit COGS" },
                    new JournalLine { AccountCode = "2000", AccountId = "5", CreditAmount = 10000, Narration = "Credit Accounts Payable" }
                }
            },
            new JournalEntry
            {
                Description = "Pending salary payment",
                ReferenceNumber = "JE-004",
                EntryDate = DateTime.UtcNow,
                IsPosted = false,
                Lines = new()
                {
                    new JournalLine { AccountCode = "5100", AccountId = "6", DebitAmount = 5000, Narration = "Debit Salary Expense" },
                    new JournalLine { AccountCode = "2100", AccountId = "7", CreditAmount = 5000, Narration = "Credit Short Term Loans" }
                }
            }
        });
    }
}
