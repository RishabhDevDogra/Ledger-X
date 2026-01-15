using System.Security.Cryptography;
using LedgerX.DTOs;
using LedgerX.Models;
using LedgerX.Repositories;

namespace LedgerX.Services;

/// <summary>
/// LedgerKey service implementation for encryption key management
/// </summary>
public class LedgerKeyService : ILedgerKeyService
{
    private readonly ILedgerKeyRepository _ledgerKeyRepository;
    private readonly ILogger<LedgerKeyService> _logger;

    public LedgerKeyService(ILedgerKeyRepository ledgerKeyRepository, ILogger<LedgerKeyService> logger)
    {
        _ledgerKeyRepository = ledgerKeyRepository ?? throw new ArgumentNullException(nameof(ledgerKeyRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<LedgerKeyDto>> GetAllKeysAsync()
    {
        _logger.LogInformation("Retrieving all ledger keys");
        var keys = await _ledgerKeyRepository.GetAllAsync();
        return keys.Select(MapToDto);
    }

    public async Task<LedgerKeyDto?> GetKeyByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            _logger.LogWarning("GetKeyById called with empty id");
            return null;
        }

        var key = await _ledgerKeyRepository.GetByIdAsync(id);
        if (key == null)
        {
            _logger.LogWarning("Ledger key with id {KeyId} not found", id);
            return null;
        }

        return MapToDto(key);
    }

    public async Task<IEnumerable<LedgerKeyDto>> GetActiveKeysAsync()
    {
        _logger.LogInformation("Retrieving active ledger keys");
        var keys = await _ledgerKeyRepository.GetActiveKeysAsync();
        return keys.Select(MapToDto);
    }

    public async Task<IEnumerable<LedgerKeyDto>> GetExpiredKeysAsync()
    {
        _logger.LogInformation("Retrieving expired ledger keys");
        var keys = await _ledgerKeyRepository.GetExpiredKeysAsync();
        return keys.Select(MapToDto);
    }

    public async Task<LedgerKeyDto> CreateKeyAsync(CreateLedgerKeyDto request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        ValidateCreateKeyRequest(request);

        var key = new LedgerKey
        {
            KeyName = request.KeyName,
            EncryptionKey = GenerateEncryptionKey(),
            ExpiresAt = request.ExpiresAt,
            IsActive = true
        };

        var createdKey = await _ledgerKeyRepository.AddAsync(key);
        _logger.LogInformation("Ledger key created with id {KeyId}", createdKey.Id);

        return MapToDto(createdKey);
    }

    public async Task<LedgerKeyDto?> RotateKeyAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentNullException(nameof(id));

        var key = await _ledgerKeyRepository.GetByIdAsync(id);
        if (key == null)
        {
            _logger.LogWarning("Cannot rotate: Ledger key with id {KeyId} not found", id);
            return null;
        }

        key.EncryptionKey = GenerateEncryptionKey();
        var updatedKey = await _ledgerKeyRepository.UpdateAsync(key);
        _logger.LogInformation("Ledger key {KeyId} rotated", id);

        return MapToDto(updatedKey);
    }

    public async Task<LedgerKeyDto?> DeactivateKeyAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentNullException(nameof(id));

        var key = await _ledgerKeyRepository.GetByIdAsync(id);
        if (key == null)
        {
            _logger.LogWarning("Cannot deactivate: Ledger key with id {KeyId} not found", id);
            return null;
        }

        key.IsActive = false;
        var updatedKey = await _ledgerKeyRepository.UpdateAsync(key);
        _logger.LogInformation("Ledger key {KeyId} deactivated", id);

        return MapToDto(updatedKey);
    }

    public async Task<bool> DeleteKeyAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentNullException(nameof(id));

        var exists = await _ledgerKeyRepository.ExistsAsync(id);
        if (!exists)
        {
            _logger.LogWarning("Cannot delete: Ledger key with id {KeyId} not found", id);
            return false;
        }

        var result = await _ledgerKeyRepository.DeleteAsync(id);
        if (result)
            _logger.LogInformation("Ledger key {KeyId} deleted", id);

        return result;
    }

    private static LedgerKeyDto MapToDto(LedgerKey key)
    {
        return new LedgerKeyDto
        {
            Id = key.Id,
            KeyName = key.KeyName,
            CreatedAt = key.CreatedAt,
            ExpiresAt = key.ExpiresAt,
            IsActive = key.IsActive
        };
    }

    private static void ValidateCreateKeyRequest(CreateLedgerKeyDto request)
    {
        if (string.IsNullOrWhiteSpace(request.KeyName))
            throw new ArgumentException("Key name is required", nameof(request.KeyName));

        if (request.ExpiresAt.HasValue && request.ExpiresAt <= DateTime.UtcNow)
            throw new ArgumentException("Expiration date must be in the future", nameof(request.ExpiresAt));
    }

    private static string GenerateEncryptionKey()
    {
        using var rng = RandomNumberGenerator.Create();
        byte[] keyBytes = new byte[32];
        rng.GetBytes(keyBytes);
        return Convert.ToBase64String(keyBytes);
    }
}
