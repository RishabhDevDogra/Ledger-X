namespace LedgerX.Models;

/// <summary>
/// Represents a trial balance report
/// </summary>
public class TrialBalance
{
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal DebitTotal { get; set; }
    public decimal CreditTotal { get; set; }
}
