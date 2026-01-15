using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;
using LedgerX.Models;
using LedgerX.Repositories;

namespace LedgerX.Tests.Repositories;

public class LedgerKeyRepositoryTests
{
    private readonly LedgerKeyRepository _repository;

    public LedgerKeyRepositoryTests()
    {
        _repository = new LedgerKeyRepository();
    }

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ReturnsSampleKeys()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsKey()
    {
        // Arrange
        var allKeys = await _repository.GetAllAsync();
        var keyId = allKeys.First().Id;

        // Act
        var result = await _repository.GetByIdAsync(keyId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(keyId);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdAsync("invalid-id");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetActiveKeysAsync Tests

    [Fact]
    public async Task GetActiveKeysAsync_ReturnsOnlyActiveKeys()
    {
        // Act
        var result = await _repository.GetActiveKeysAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().AllSatisfy(k => k.IsActive.Should().BeTrue());
    }

    [Fact]
    public async Task GetActiveKeysAsync_ExcludesExpiredKeys()
    {
        // Act
        var result = await _repository.GetActiveKeysAsync();

        // Assert
        result.Should().AllSatisfy(k => 
        {
            if (k.ExpiresAt.HasValue)
            {
                k.ExpiresAt.Value.Should().BeOnOrAfter(DateTime.UtcNow.AddSeconds(-1));
            }
        });
    }

    #endregion

    #region GetExpiredKeysAsync Tests

    [Fact]
    public async Task GetExpiredKeysAsync_ReturnsOnlyExpiredKeys()
    {
        // Act
        var result = await _repository.GetExpiredKeysAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().AllSatisfy(k => 
        {
            k.ExpiresAt.Should().NotBeNull();
            k.ExpiresAt.Value.Should().BeOnOrBefore(DateTime.UtcNow.AddSeconds(1));
        });
    }

    #endregion

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_WithValidKey_AddsAndReturns()
    {
        // Arrange
        var key = new LedgerKey
        {
            KeyName = "Test Key",
            EncryptionKey = Convert.ToBase64String(new byte[32]),
            ExpiresAt = DateTime.UtcNow.AddYears(1)
        };

        // Act
        var result = await _repository.AddAsync(key);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeNullOrEmpty();
        var addedKey = await _repository.GetByIdAsync(result.Id);
        addedKey.Should().NotBeNull();
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithValidKey_UpdatesAndReturns()
    {
        // Arrange
        var allKeys = await _repository.GetAllAsync();
        var key = allKeys.First();
        key.IsActive = false;

        // Act
        var result = await _repository.UpdateAsync(key);

        // Assert
        result.IsActive.Should().BeFalse();
        var updatedKey = await _repository.GetByIdAsync(key.Id);
        updatedKey!.IsActive.Should().BeFalse();
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WithValidId_Deletes()
    {
        // Arrange
        var newKey = await _repository.AddAsync(new LedgerKey
        {
            KeyName = "To Delete",
            EncryptionKey = Convert.ToBase64String(new byte[32])
        });

        // Act
        var result = await _repository.DeleteAsync(newKey.Id);

        // Assert
        result.Should().BeTrue();
        var deletedKey = await _repository.GetByIdAsync(newKey.Id);
        deletedKey.Should().BeNull();
    }

    #endregion

    #region ExistsAsync Tests

    [Fact]
    public async Task ExistsAsync_WithValidId_ReturnsTrue()
    {
        // Arrange
        var allKeys = await _repository.GetAllAsync();
        var keyId = allKeys.First().Id;

        // Act
        var result = await _repository.ExistsAsync(keyId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithInvalidId_ReturnsFalse()
    {
        // Act
        var result = await _repository.ExistsAsync("invalid-id");

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
