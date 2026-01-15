using Microsoft.AspNetCore.Mvc;
using LedgerX.DTOs;
using LedgerX.Services;

namespace LedgerX.Controllers;

/// <summary>
/// Controller for Account operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly ILogger<AccountsController> _logger;

    public AccountsController(IAccountService accountService, ILogger<AccountsController> logger)
    {
        _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get all accounts
    /// </summary>
    /// <returns>List of all accounts</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AccountDto>>> GetAllAccounts()
    {
        _logger.LogDebug("GetAllAccounts endpoint called");
        var accounts = await _accountService.GetAllAccountsAsync();
        return Ok(accounts);
    }

    /// <summary>
    /// Get account by ID
    /// </summary>
    /// <param name="id">Account ID</param>
    /// <returns>Account details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountDto>> GetAccountById(string id)
    {
        _logger.LogDebug("GetAccountById endpoint called with id {AccountId}", id);
        var account = await _accountService.GetAccountByIdAsync(id);
        
        if (account == null)
            return NotFound(new { message = $"Account with id {id} not found" });

        return Ok(account);
    }

    /// <summary>
    /// Get account by code
    /// </summary>
    /// <param name="code">Account code</param>
    /// <returns>Account details</returns>
    [HttpGet("by-code/{code}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountDto>> GetAccountByCode(string code)
    {
        _logger.LogDebug("GetAccountByCode endpoint called with code {AccountCode}", code);
        var account = await _accountService.GetAccountByCodeAsync(code);
        
        if (account == null)
            return NotFound(new { message = $"Account with code {code} not found" });

        return Ok(account);
    }

    /// <summary>
    /// Get all active accounts
    /// </summary>
    /// <returns>List of active accounts</returns>
    [HttpGet("active")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AccountDto>>> GetActiveAccounts()
    {
        _logger.LogDebug("GetActiveAccounts endpoint called");
        var accounts = await _accountService.GetActiveAccountsAsync();
        return Ok(accounts);
    }

    /// <summary>
    /// Create a new account
    /// </summary>
    /// <param name="request">Account creation request</param>
    /// <returns>Created account</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AccountDto>> CreateAccount([FromBody] CreateAccountDto request)
    {
        try
        {
            _logger.LogInformation("CreateAccount endpoint called with code {AccountCode}", request.Code);
            var account = await _accountService.CreateAccountAsync(request);
            return CreatedAtAction(nameof(GetAccountById), new { id = account.Id }, account);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Validation error in CreateAccount: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Conflict in CreateAccount: {Message}", ex.Message);
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update an account
    /// </summary>
    /// <param name="id">Account ID</param>
    /// <param name="request">Update request</param>
    /// <returns>Updated account</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AccountDto>> UpdateAccount(string id, [FromBody] UpdateAccountDto request)
    {
        try
        {
            _logger.LogInformation("UpdateAccount endpoint called for id {AccountId}", id);
            var account = await _accountService.UpdateAccountAsync(id, request);
            
            if (account == null)
                return NotFound(new { message = $"Account with id {id} not found" });

            return Ok(account);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Validation error in UpdateAccount: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete an account
    /// </summary>
    /// <param name="id">Account ID</param>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteAccount(string id)
    {
        _logger.LogInformation("DeleteAccount endpoint called for id {AccountId}", id);
        var result = await _accountService.DeleteAccountAsync(id);

        if (!result)
            return NotFound(new { message = $"Account with id {id} not found" });

        return NoContent();
    }
}
