namespace LedgerX.DTOs;

/// <summary>
/// Response DTO for Account entity
/// </summary>
public record AccountDto
{
    public string Id { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public decimal Balance { get; init; }
    public DateTime CreatedAt { get; init; }
    public bool IsActive { get; init; }
}

/// <summary>
/// Request DTO for creating an account
/// </summary>
public record CreateAccountDto
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public decimal Balance { get; init; }
}

/// <summary>
/// Request DTO for updating an account
/// </summary>
public record UpdateAccountDto
{
    public string? Name { get; init; }
    public string? Type { get; init; }
    public bool? IsActive { get; init; }
}
