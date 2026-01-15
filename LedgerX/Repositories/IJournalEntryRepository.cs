using LedgerX.Models;

namespace LedgerX.Repositories;

/// <summary>
/// Repository interface for JournalEntry entity
/// </summary>
public interface IJournalEntryRepository : IRepository<JournalEntry>
{
    Task<IEnumerable<JournalEntry>> GetPostedEntriesAsync();
    Task<IEnumerable<JournalEntry>> GetEntriesByDateRangeAsync(DateTime startDate, DateTime endDate);
}
