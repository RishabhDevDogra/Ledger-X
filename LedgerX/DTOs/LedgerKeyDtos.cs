namespace LedgerX.DTOs;

public record LedgerKeyDto
{
    public string Id { get; init; } = string.Empty;
    public string KeyName { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public bool IsActive { get; init; }
}

public record CreateLedgerKeyDto
{
    public string KeyName { get; init; } = string.Empty;
    public DateTime? ExpiresAt { get; init; }
}

public record UpdateLedgerKeyDto
{
    public string? KeyName { get; init; }
    public DateTime? ExpiresAt { get; init; }
}
