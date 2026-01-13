namespace LedgerX.Models;

/// <summary>
/// Represents a ledger account (Asset, Liability, Equity, Revenue, Expense)
/// </summary>
public class Account
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Asset, Liability, Equity, Revenue, Expense
    public decimal Balance { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}
