using Microsoft.AspNetCore.Mvc;
using LedgerX.DTOs;
using LedgerX.Services;

namespace LedgerX.Controllers;

/// <summary>
/// Controller for accounting reports
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(IReportService reportService, ILogger<ReportsController> logger)
    {
        _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Generate a trial balance report
    /// </summary>
    /// <returns>Trial balance showing all accounts with debit and credit totals</returns>
    [HttpGet("trial-balance")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<TrialBalanceReportDto>> GetTrialBalance()
    {
        _logger.LogInformation("GetTrialBalance endpoint called");
        var report = await _reportService.GetTrialBalanceAsync();
        return Ok(report);
    }

    /// <summary>
    /// Get total debits from all posted journal entries
    /// </summary>
    /// <returns>Total debit amount</returns>
    [HttpGet("total-debits")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<object>> GetTotalDebits()
    {
        _logger.LogInformation("GetTotalDebits endpoint called");
        var totalDebits = await _reportService.GetTotalDebitAsync();
        return Ok(new { totalDebits });
    }

    /// <summary>
    /// Get total credits from all posted journal entries
    /// </summary>
    /// <returns>Total credit amount</returns>
    [HttpGet("total-credits")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<object>> GetTotalCredits()
    {
        _logger.LogInformation("GetTotalCredits endpoint called");
        var totalCredits = await _reportService.GetTotalCreditAsync();
        return Ok(new { totalCredits });
    }
}
