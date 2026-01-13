namespace LedgerX.Models;

/// <summary>
/// Represents a journal entry with double-entry bookkeeping
/// </summary>
public class JournalEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Description { get; set; } = string.Empty;
    public DateTime EntryDate { get; set; } = DateTime.UtcNow;
    public string ReferenceNumber { get; set; } = string.Empty;
    public List<JournalLine> Lines { get; set; } = new();
    public bool IsPosted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Represents a single line item in a journal entry (debit or credit)
/// </summary>
public class JournalLine
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string AccountId { get; set; } = string.Empty;
    public string AccountCode { get; set; } = string.Empty;
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public string Narration { get; set; } = string.Empty;
}
