using Microsoft.AspNetCore.Mvc;
using LedgerX.DTOs;
using LedgerX.Services;

namespace LedgerX.Controllers;

/// <summary>
/// Controller for Ledger Key management
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class LedgerKeysController : ControllerBase
{
    private readonly ILedgerKeyService _ledgerKeyService;
    private readonly ILogger<LedgerKeysController> _logger;

    public LedgerKeysController(ILedgerKeyService ledgerKeyService, ILogger<LedgerKeysController> logger)
    {
        _ledgerKeyService = ledgerKeyService ?? throw new ArgumentNullException(nameof(ledgerKeyService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get all ledger keys
    /// </summary>
    /// <returns>List of all ledger keys</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<LedgerKeyDto>>> GetAllKeys()
    {
        _logger.LogDebug("GetAllKeys endpoint called");
        var keys = await _ledgerKeyService.GetAllKeysAsync();
        return Ok(keys);
    }

    /// <summary>
    /// Get ledger key by ID
    /// </summary>
    /// <param name="id">Ledger key ID</param>
    /// <returns>Ledger key details (without sensitive data)</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LedgerKeyDto>> GetKeyById(string id)
    {
        _logger.LogDebug("GetKeyById endpoint called with id {KeyId}", id);
        var key = await _ledgerKeyService.GetKeyByIdAsync(id);
        
        if (key == null)
            return NotFound(new { message = $"Ledger key with id {id} not found" });

        return Ok(key);
    }

    /// <summary>
    /// Get all active ledger keys
    /// </summary>
    /// <returns>List of active ledger keys</returns>
    [HttpGet("active")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<LedgerKeyDto>>> GetActiveKeys()
    {
        _logger.LogDebug("GetActiveKeys endpoint called");
        var keys = await _ledgerKeyService.GetActiveKeysAsync();
        return Ok(keys);
    }

    /// <summary>
    /// Get all expired ledger keys
    /// </summary>
    /// <returns>List of expired ledger keys</returns>
    [HttpGet("expired")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<LedgerKeyDto>>> GetExpiredKeys()
    {
        _logger.LogDebug("GetExpiredKeys endpoint called");
        var keys = await _ledgerKeyService.GetExpiredKeysAsync();
        return Ok(keys);
    }

    /// <summary>
    /// Create a new ledger encryption key
    /// </summary>
    /// <param name="request">Key creation request</param>
    /// <returns>Created ledger key</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LedgerKeyDto>> CreateKey([FromBody] CreateLedgerKeyDto request)
    {
        try
        {
            _logger.LogInformation("CreateKey endpoint called with name {KeyName}", request.KeyName);
            var key = await _ledgerKeyService.CreateKeyAsync(request);
            return CreatedAtAction(nameof(GetKeyById), new { id = key.Id }, key);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Validation error in CreateKey: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Rotate/refresh a ledger key
    /// </summary>
    /// <param name="id">Ledger key ID</param>
    [HttpPost("{id}/rotate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LedgerKeyDto>> RotateKey(string id)
    {
        _logger.LogInformation("RotateKey endpoint called for id {KeyId}", id);
        var key = await _ledgerKeyService.RotateKeyAsync(id);
        
        if (key == null)
            return NotFound(new { message = $"Ledger key with id {id} not found" });

        return Ok(key);
    }

    /// <summary>
    /// Deactivate a ledger key
    /// </summary>
    /// <param name="id">Ledger key ID</param>
    [HttpPost("{id}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LedgerKeyDto>> DeactivateKey(string id)
    {
        _logger.LogInformation("DeactivateKey endpoint called for id {KeyId}", id);
        var key = await _ledgerKeyService.DeactivateKeyAsync(id);
        
        if (key == null)
            return NotFound(new { message = $"Ledger key with id {id} not found" });

        return Ok(key);
    }

    /// <summary>
    /// Delete a ledger key
    /// </summary>
    /// <param name="id">Ledger key ID</param>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteKey(string id)
    {
        _logger.LogInformation("DeleteKey endpoint called for id {KeyId}", id);
        var result = await _ledgerKeyService.DeleteKeyAsync(id);

        if (!result)
            return NotFound(new { message = $"Ledger key with id {id} not found" });

        return NoContent();
    }
}
