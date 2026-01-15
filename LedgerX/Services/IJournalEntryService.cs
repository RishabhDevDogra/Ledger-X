using LedgerX.DTOs;

namespace LedgerX.Services;

/// <summary>
/// Service interface for JournalEntry business logic
/// </summary>
public interface IJournalEntryService
{
    Task<IEnumerable<JournalEntryDto>> GetAllEntriesAsync();
    Task<JournalEntryDto?> GetEntryByIdAsync(string id);
    Task<IEnumerable<JournalEntryDto>> GetPostedEntriesAsync();
    Task<IEnumerable<JournalEntryDto>> GetEntriesByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<JournalEntryDto> CreateJournalEntryAsync(CreateJournalEntryDto request);
    Task<JournalEntryDto?> UpdateJournalEntryAsync(string id, UpdateJournalEntryDto request);
    Task<bool> PostJournalEntryAsync(string id);
    Task<bool> DeleteJournalEntryAsync(string id);
}
