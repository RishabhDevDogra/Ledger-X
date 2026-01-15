namespace LedgerX.DTOs;

public record TrialBalanceDto
{
    public string AccountCode { get; init; } = string.Empty;
    public string AccountName { get; init; } = string.Empty;
    public decimal Debit { get; init; }
    public decimal Credit { get; init; }
}

public record TrialBalanceReportDto
{
    public List<TrialBalanceDto> Accounts { get; init; } = new();
    public decimal TotalDebits { get; init; }
    public decimal TotalCredits { get; init; }
    public bool IsBalanced { get; init; }
}
