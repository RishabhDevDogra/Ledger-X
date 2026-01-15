using LedgerX.DTOs;
using LedgerX.Models;
using LedgerX.Repositories;

namespace LedgerX.Services;

/// <summary>
/// Journal Entry service implementation with business logic and validation
/// </summary>
public class JournalEntryService : IJournalEntryService
{
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<JournalEntryService> _logger;

    public JournalEntryService(
        IJournalEntryRepository journalEntryRepository,
        IAccountRepository accountRepository,
        ILogger<JournalEntryService> logger)
    {
        _journalEntryRepository = journalEntryRepository ?? throw new ArgumentNullException(nameof(journalEntryRepository));
        _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<JournalEntryDto>> GetAllEntriesAsync()
    {
        _logger.LogInformation("Retrieving all journal entries");
        var entries = await _journalEntryRepository.GetAllAsync();
        return entries.Select(MapToDto);
    }

    public async Task<JournalEntryDto?> GetEntryByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            _logger.LogWarning("GetEntryById called with empty id");
            return null;
        }

        var entry = await _journalEntryRepository.GetByIdAsync(id);
        if (entry == null)
        {
            _logger.LogWarning("Journal entry with id {EntryId} not found", id);
            return null;
        }

        return MapToDto(entry);
    }

    public async Task<IEnumerable<JournalEntryDto>> GetPostedEntriesAsync()
    {
        _logger.LogInformation("Retrieving posted journal entries");
        var entries = await _journalEntryRepository.GetPostedEntriesAsync();
        return entries.Select(MapToDto);
    }

    public async Task<IEnumerable<JournalEntryDto>> GetEntriesByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        if (startDate > endDate)
        {
            _logger.LogWarning("Invalid date range: start date {StartDate} is after end date {EndDate}", startDate, endDate);
            throw new ArgumentException("Start date must be less than or equal to end date");
        }

        _logger.LogInformation("Retrieving journal entries between {StartDate} and {EndDate}", startDate, endDate);
        var entries = await _journalEntryRepository.GetEntriesByDateRangeAsync(startDate, endDate);
        return entries.Select(MapToDto);
    }

    public async Task<JournalEntryDto> CreateJournalEntryAsync(CreateJournalEntryDto request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        ValidateCreateJournalEntryRequest(request);

        var entry = new JournalEntry
        {
            Description = request.Description,
            ReferenceNumber = request.ReferenceNumber,
            EntryDate = request.EntryDate,
            IsPosted = false,
            Lines = request.Lines.Select(line => new JournalLine
            {
                AccountCode = line.AccountCode,
                AccountId = line.AccountId,
                DebitAmount = line.DebitAmount,
                CreditAmount = line.CreditAmount,
                Narration = line.Narration
            }).ToList()
        };

        var createdEntry = await _journalEntryRepository.AddAsync(entry);
        _logger.LogInformation("Journal entry created with id {EntryId}", createdEntry.Id);

        return MapToDto(createdEntry);
    }

    public async Task<JournalEntryDto?> UpdateJournalEntryAsync(string id, UpdateJournalEntryDto request)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentNullException(nameof(id));

        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var entry = await _journalEntryRepository.GetByIdAsync(id);
        if (entry == null)
        {
            _logger.LogWarning("Cannot update: Journal entry with id {EntryId} not found", id);
            return null;
        }

        // Prevent updating posted entries
        if (entry.IsPosted)
        {
            _logger.LogWarning("Cannot update posted journal entry {EntryId}", id);
            throw new InvalidOperationException("Cannot update a posted journal entry");
        }

        if (!string.IsNullOrWhiteSpace(request.Description))
            entry.Description = request.Description;

        var updatedEntry = await _journalEntryRepository.UpdateAsync(entry);
        _logger.LogInformation("Journal entry {EntryId} updated", id);

        return MapToDto(updatedEntry);
    }

    public async Task<bool> PostJournalEntryAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentNullException(nameof(id));

        var entry = await _journalEntryRepository.GetByIdAsync(id);
        if (entry == null)
        {
            _logger.LogWarning("Cannot post: Journal entry with id {EntryId} not found", id);
            return false;
        }

        if (entry.IsPosted)
        {
            _logger.LogWarning("Journal entry {EntryId} is already posted", id);
            return false;
        }

        entry.IsPosted = true;
        await _journalEntryRepository.UpdateAsync(entry);
        _logger.LogInformation("Journal entry {EntryId} posted", id);

        return true;
    }

    public async Task<bool> DeleteJournalEntryAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentNullException(nameof(id));

        var entry = await _journalEntryRepository.GetByIdAsync(id);
        if (entry == null)
        {
            _logger.LogWarning("Cannot delete: Journal entry with id {EntryId} not found", id);
            return false;
        }

        // Prevent deleting posted entries
        if (entry.IsPosted)
        {
            _logger.LogWarning("Cannot delete posted journal entry {EntryId}", id);
            throw new InvalidOperationException("Cannot delete a posted journal entry");
        }

        var result = await _journalEntryRepository.DeleteAsync(id);
        if (result)
            _logger.LogInformation("Journal entry {EntryId} deleted", id);

        return result;
    }

    private static JournalEntryDto MapToDto(JournalEntry entry)
    {
        return new JournalEntryDto
        {
            Id = entry.Id,
            Description = entry.Description,
            ReferenceNumber = entry.ReferenceNumber,
            EntryDate = entry.EntryDate,
            IsPosted = entry.IsPosted,
            Lines = entry.Lines.Select(line => new JournalLineDto
            {
                AccountCode = line.AccountCode,
                AccountId = line.AccountId,
                DebitAmount = line.DebitAmount,
                CreditAmount = line.CreditAmount,
                Narration = line.Narration
            }).ToList(),
            CreatedAt = entry.CreatedAt
        };
    }

    private static void ValidateCreateJournalEntryRequest(CreateJournalEntryDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Description))
            throw new ArgumentException("Journal entry description is required", nameof(request.Description));

        if (string.IsNullOrWhiteSpace(request.ReferenceNumber))
            throw new ArgumentException("Journal entry reference number is required", nameof(request.ReferenceNumber));

        if (request.Lines == null || request.Lines.Count < 2)
            throw new ArgumentException("Journal entry must have at least 2 lines (debit and credit)", nameof(request.Lines));

        // Verify double-entry principle
        decimal totalDebits = request.Lines.Sum(l => l.DebitAmount);
        decimal totalCredits = request.Lines.Sum(l => l.CreditAmount);

        if (Math.Abs(totalDebits - totalCredits) > 0.01m) // Allow for floating point precision
        {
            throw new InvalidOperationException(
                $"Journal entry does not balance. Total debits: {totalDebits}, Total credits: {totalCredits}");
        }

        // Validate each line
        foreach (var line in request.Lines)
        {
            if (string.IsNullOrWhiteSpace(line.AccountCode))
                throw new ArgumentException("Account code is required for each journal line");

            if (line.DebitAmount < 0 || line.CreditAmount < 0)
                throw new ArgumentException("Debit and credit amounts must be non-negative");

            if (line.DebitAmount > 0 && line.CreditAmount > 0)
                throw new ArgumentException("A journal line cannot have both debit and credit amounts");
        }
    }
}
