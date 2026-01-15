using LedgerX.DTOs;

namespace LedgerX.Services;

/// <summary>
/// Service interface for LedgerKey operations
/// </summary>
public interface ILedgerKeyService
{
    Task<IEnumerable<LedgerKeyDto>> GetAllKeysAsync();
    Task<LedgerKeyDto?> GetKeyByIdAsync(string id);
    Task<IEnumerable<LedgerKeyDto>> GetActiveKeysAsync();
    Task<IEnumerable<LedgerKeyDto>> GetExpiredKeysAsync();
    Task<LedgerKeyDto> CreateKeyAsync(CreateLedgerKeyDto request);
    Task<LedgerKeyDto?> RotateKeyAsync(string id);
    Task<LedgerKeyDto?> DeactivateKeyAsync(string id);
    Task<bool> DeleteKeyAsync(string id);
}
