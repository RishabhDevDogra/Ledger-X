namespace LedgerX.DTOs;

public record JournalLineDto
{
    public string AccountCode { get; init; } = string.Empty;
    public string AccountId { get; init; } = string.Empty;
    public decimal DebitAmount { get; init; }
    public decimal CreditAmount { get; init; }
    public string Narration { get; init; } = string.Empty;
}

public record JournalEntryDto
{
    public string Id { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string ReferenceNumber { get; init; } = string.Empty;
    public DateTime EntryDate { get; init; }
    public bool IsPosted { get; init; }
    public List<JournalLineDto> Lines { get; init; } = new();
    public DateTime CreatedAt { get; init; }
}

public record CreateJournalEntryDto
{
    public string Description { get; init; } = string.Empty;
    public string ReferenceNumber { get; init; } = string.Empty;
    public DateTime EntryDate { get; init; }
    public List<JournalLineDto> Lines { get; init; } = new();
}

public record UpdateJournalEntryDto
{
    public string? Description { get; init; }
    public bool? IsPosted { get; init; }
}
