using LedgerX.DTOs;

namespace LedgerX.Services;

/// <summary>
/// Service interface for accounting reports
/// </summary>
public interface IReportService
{
    Task<TrialBalanceReportDto> GetTrialBalanceAsync();
    Task<decimal> GetTotalDebitAsync();
    Task<decimal> GetTotalCreditAsync();
}
