using Microsoft.AspNetCore.Mvc;
using LedgerX.Models;

namespace LedgerX.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class JournalEntriesController : ControllerBase
{
    private static List<JournalEntry> _journalEntries = new()
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
    };

    /// <summary>
    /// Get all journal entries
    /// </summary>
    /// <returns>List of all journal entries</returns>
    [HttpGet]
    public ActionResult<IEnumerable<JournalEntry>> GetAllEntries()
    {
        return Ok(_journalEntries);
    }

    /// <summary>
    /// Get journal entry by ID
    /// </summary>
    /// <param name="id">Journal entry ID</param>
    /// <returns>Journal entry details</returns>
    [HttpGet("{id}")]
    public ActionResult<JournalEntry> GetEntryById(string id)
    {
        var entry = _journalEntries.FirstOrDefault(e => e.Id == id);
        if (entry == null)
            return NotFound(new { message = "Journal entry not found" });

        return Ok(entry);
    }

    /// <summary>
    /// Create a new journal entry with double-entry bookkeeping
    /// </summary>
    /// <param name="request">Journal entry creation request</param>
    /// <returns>Created journal entry</returns>
    [HttpPost]
    public ActionResult<JournalEntry> CreateJournalEntry([FromBody] CreateJournalEntryRequest request)
    {
        if (request.Lines == null || request.Lines.Count < 2)
            return BadRequest(new { message = "Journal entry must have at least 2 lines (debit and credit)" });

        // Verify double-entry principle: total debits = total credits
        decimal totalDebits = request.Lines.Sum(l => l.DebitAmount);
        decimal totalCredits = request.Lines.Sum(l => l.CreditAmount);

        if (totalDebits != totalCredits)
            return BadRequest(new { 
                message = "Double-entry bookkeeping violation: Total debits must equal total credits",
                totalDebits,
                totalCredits
            });

        var entry = new JournalEntry
        {
            Description = request.Description,
            ReferenceNumber = request.ReferenceNumber,
            EntryDate = request.EntryDate ?? DateTime.UtcNow,
            Lines = request.Lines
        };

        _journalEntries.Add(entry);
        return CreatedAtAction(nameof(GetEntryById), new { id = entry.Id }, entry);
    }

    /// <summary>
    /// Post/approve a journal entry
    /// </summary>
    /// <param name="id">Journal entry ID</param>
    [HttpPost("{id}/post")]
    public ActionResult<JournalEntry> PostJournalEntry(string id)
    {
        var entry = _journalEntries.FirstOrDefault(e => e.Id == id);
        if (entry == null)
            return NotFound(new { message = "Journal entry not found" });

        if (entry.IsPosted)
            return BadRequest(new { message = "Journal entry is already posted" });

        entry.IsPosted = true;
        return Ok(entry);
    }

    /// <summary>
    /// Delete a journal entry (only if not posted)
    /// </summary>
    /// <param name="id">Journal entry ID</param>
    [HttpDelete("{id}")]
    public ActionResult DeleteJournalEntry(string id)
    {
        var entry = _journalEntries.FirstOrDefault(e => e.Id == id);
        if (entry == null)
            return NotFound(new { message = "Journal entry not found" });

        if (entry.IsPosted)
            return BadRequest(new { message = "Cannot delete a posted journal entry" });

        _journalEntries.Remove(entry);
        return NoContent();
    }
}

/// <summary>
/// Request model for creating a journal entry
/// </summary>
public class CreateJournalEntryRequest
{
    public string Description { get; set; } = string.Empty;
    public string ReferenceNumber { get; set; } = string.Empty;
    public DateTime? EntryDate { get; set; }
    public List<JournalLine> Lines { get; set; } = new();
}
