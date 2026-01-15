using Microsoft.AspNetCore.Mvc;
using LedgerX.DTOs;
using LedgerX.Services;

namespace LedgerX.Controllers;

/// <summary>
/// Controller for Journal Entry operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class JournalEntriesController : ControllerBase
{
    private readonly IJournalEntryService _journalEntryService;
    private readonly ILogger<JournalEntriesController> _logger;

    public JournalEntriesController(IJournalEntryService journalEntryService, ILogger<JournalEntriesController> logger)
    {
        _journalEntryService = journalEntryService ?? throw new ArgumentNullException(nameof(journalEntryService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get all journal entries
    /// </summary>
    /// <returns>List of all journal entries</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<JournalEntryDto>>> GetAllEntries()
    {
        _logger.LogDebug("GetAllEntries endpoint called");
        var entries = await _journalEntryService.GetAllEntriesAsync();
        return Ok(entries);
    }

    /// <summary>
    /// Get journal entry by ID
    /// </summary>
    /// <param name="id">Journal entry ID</param>
    /// <returns>Journal entry details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<JournalEntryDto>> GetEntryById(string id)
    {
        _logger.LogDebug("GetEntryById endpoint called with id {EntryId}", id);
        var entry = await _journalEntryService.GetEntryByIdAsync(id);
        
        if (entry == null)
            return NotFound(new { message = $"Journal entry with id {id} not found" });

        return Ok(entry);
    }

    /// <summary>
    /// Get all posted journal entries
    /// </summary>
    /// <returns>List of posted journal entries</returns>
    [HttpGet("posted")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<JournalEntryDto>>> GetPostedEntries()
    {
        _logger.LogDebug("GetPostedEntries endpoint called");
        var entries = await _journalEntryService.GetPostedEntriesAsync();
        return Ok(entries);
    }

    /// <summary>
    /// Get journal entries by date range
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>List of journal entries within date range</returns>
    [HttpGet("by-date-range")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<JournalEntryDto>>> GetEntriesByDateRange(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            _logger.LogDebug("GetEntriesByDateRange endpoint called with startDate {StartDate} and endDate {EndDate}", startDate, endDate);
            var entries = await _journalEntryService.GetEntriesByDateRangeAsync(startDate, endDate);
            return Ok(entries);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Validation error in GetEntriesByDateRange: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Create a new journal entry with double-entry bookkeeping
    /// </summary>
    /// <param name="request">Journal entry creation request</param>
    /// <returns>Created journal entry</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<JournalEntryDto>> CreateJournalEntry([FromBody] CreateJournalEntryDto request)
    {
        try
        {
            _logger.LogInformation("CreateJournalEntry endpoint called with reference {ReferenceNumber}", request.ReferenceNumber);
            var entry = await _journalEntryService.CreateJournalEntryAsync(request);
            return CreatedAtAction(nameof(GetEntryById), new { id = entry.Id }, entry);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Validation error in CreateJournalEntry: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Business logic error in CreateJournalEntry: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update a journal entry (only if not posted)
    /// </summary>
    /// <param name="id">Journal entry ID</param>
    /// <param name="request">Update request</param>
    /// <returns>Updated journal entry</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<JournalEntryDto>> UpdateJournalEntry(string id, [FromBody] UpdateJournalEntryDto request)
    {
        try
        {
            _logger.LogInformation("UpdateJournalEntry endpoint called for id {EntryId}", id);
            var entry = await _journalEntryService.UpdateJournalEntryAsync(id, request);
            
            if (entry == null)
                return NotFound(new { message = $"Journal entry with id {id} not found" });

            return Ok(entry);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Validation error in UpdateJournalEntry: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Business logic error in UpdateJournalEntry: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Post/approve a journal entry
    /// </summary>
    /// <param name="id">Journal entry ID</param>
    [HttpPost("{id}/post")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> PostJournalEntry(string id)
    {
        try
        {
            _logger.LogInformation("PostJournalEntry endpoint called for id {EntryId}", id);
            var result = await _journalEntryService.PostJournalEntryAsync(id);

            if (!result)
                return NotFound(new { message = $"Journal entry with id {id} not found or already posted" });

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Business logic error in PostJournalEntry: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete a journal entry (only if not posted)
    /// </summary>
    /// <param name="id">Journal entry ID</param>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> DeleteJournalEntry(string id)
    {
        try
        {
            _logger.LogInformation("DeleteJournalEntry endpoint called for id {EntryId}", id);
            var result = await _journalEntryService.DeleteJournalEntryAsync(id);

            if (!result)
                return NotFound(new { message = $"Journal entry with id {id} not found" });

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Business logic error in DeleteJournalEntry: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }
}
