using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using Xunit;
using LedgerX.DTOs;
using LedgerX.Models;
using LedgerX.Repositories;
using LedgerX.Services;
using LedgerX.Tests.Fixtures;
using Microsoft.Extensions.Logging;

namespace LedgerX.Tests.Services;

public class LedgerKeyServiceTests
{
    private readonly Mock<ILedgerKeyRepository> _mockRepository;
    private readonly Mock<ILogger<LedgerKeyService>> _mockLogger;
    private readonly LedgerKeyService _service;

    public LedgerKeyServiceTests()
    {
        _mockRepository = new Mock<ILedgerKeyRepository>();
        _mockLogger = new Mock<ILogger<LedgerKeyService>>();
        _service = new LedgerKeyService(_mockRepository.Object, _mockLogger.Object);
    }

    #region GetAllKeysAsync Tests

    [Fact]
    public async Task GetAllKeysAsync_ReturnsAllKeys()
    {
        // Arrange
        var keys = new List<LedgerKey>
        {
            TestDataFixture.SampleLedgerKey,
            new() { Id = "2", KeyName = "Test Key 2", EncryptionKey = "key2", IsActive = true }
        };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(keys);

        // Act
        var result = await _service.GetAllKeysAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllKeysAsync_WhenNoKeys_ReturnsEmptyList()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<LedgerKey>());

        // Act
        var result = await _service.GetAllKeysAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetKeyByIdAsync Tests

    [Fact]
    public async Task GetKeyByIdAsync_WithValidId_ReturnsKey()
    {
        // Arrange
        var key = TestDataFixture.SampleLedgerKey;
        _mockRepository.Setup(r => r.GetByIdAsync(key.Id)).ReturnsAsync(key);

        // Act
        var result = await _service.GetKeyByIdAsync(key.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(key.Id);
    }

    [Fact]
    public async Task GetKeyByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(It.IsAny<string>())).ReturnsAsync((LedgerKey?)null);

        // Act
        var result = await _service.GetKeyByIdAsync("invalid-id");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetActiveKeysAsync Tests

    [Fact]
    public async Task GetActiveKeysAsync_ReturnsOnlyActiveKeys()
    {
        // Arrange
        var activeKeys = new List<LedgerKey>
        {
            new() { Id = "1", KeyName = "Active Key 1", IsActive = true },
            new() { Id = "2", KeyName = "Active Key 2", IsActive = true }
        };
        _mockRepository.Setup(r => r.GetActiveKeysAsync()).ReturnsAsync(activeKeys);

        // Act
        var result = await _service.GetActiveKeysAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(k => k.IsActive.Should().BeTrue());
    }

    #endregion

    #region GetExpiredKeysAsync Tests

    [Fact]
    public async Task GetExpiredKeysAsync_ReturnsOnlyExpiredKeys()
    {
        // Arrange
        var expiredKeys = new List<LedgerKey>
        {
            new() { Id = "1", KeyName = "Expired Key", ExpiresAt = DateTime.UtcNow.AddDays(-1) }
        };
        _mockRepository.Setup(r => r.GetExpiredKeysAsync()).ReturnsAsync(expiredKeys);

        // Act
        var result = await _service.GetExpiredKeysAsync();

        // Assert
        result.Should().HaveCount(1);
    }

    #endregion

    #region CreateKeyAsync Tests

    [Fact]
    public async Task CreateKeyAsync_WithValidRequest_CreatesKey()
    {
        // Arrange
        var request = TestDataFixture.ValidLedgerKeyRequest;
        var createdKey = new LedgerKey
        {
            Id = Guid.NewGuid().ToString(),
            KeyName = request.KeyName,
            EncryptionKey = Convert.ToBase64String(new byte[32]),
            ExpiresAt = request.ExpiresAt,
            IsActive = true
        };
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<LedgerKey>())).ReturnsAsync(createdKey);

        // Act
        var result = await _service.CreateKeyAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.KeyName.Should().Be(request.KeyName);
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateKeyAsync_WithMissingKeyName_ThrowsException()
    {
        // Arrange
        var request = new CreateLedgerKeyDto { KeyName = "" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateKeyAsync(request));
    }

    [Fact]
    public async Task CreateKeyAsync_WithPastExpirationDate_ThrowsException()
    {
        // Arrange
        var request = new CreateLedgerKeyDto
        {
            KeyName = "Test Key",
            ExpiresAt = DateTime.UtcNow.AddDays(-1) // Past date
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateKeyAsync(request));
    }

    #endregion

    #region RotateKeyAsync Tests

    [Fact]
    public async Task RotateKeyAsync_WithValidId_RotatesKey()
    {
        // Arrange
        var keyId = "test-id";
        var key = new LedgerKey
        {
            Id = keyId,
            KeyName = "Test Key",
            EncryptionKey = "old-key",
            IsActive = true
        };
        var rotatedKey = new LedgerKey
        {
            Id = keyId,
            KeyName = "Test Key",
            EncryptionKey = "new-key",
            IsActive = true
        };

        _mockRepository.Setup(r => r.GetByIdAsync(keyId)).ReturnsAsync(key);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<LedgerKey>())).ReturnsAsync(rotatedKey);

        // Act
        var result = await _service.RotateKeyAsync(keyId);

        // Assert
        result.Should().NotBeNull();
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<LedgerKey>()), Times.Once);
    }

    [Fact]
    public async Task RotateKeyAsync_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(It.IsAny<string>())).ReturnsAsync((LedgerKey?)null);

        // Act
        var result = await _service.RotateKeyAsync("nonexistent");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region DeactivateKeyAsync Tests

    [Fact]
    public async Task DeactivateKeyAsync_WithValidId_DeactivatesKey()
    {
        // Arrange
        var keyId = "test-id";
        var key = new LedgerKey { Id = keyId, KeyName = "Test", IsActive = true };
        var deactivatedKey = new LedgerKey { Id = keyId, KeyName = "Test", IsActive = false };

        _mockRepository.Setup(r => r.GetByIdAsync(keyId)).ReturnsAsync(key);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<LedgerKey>())).ReturnsAsync(deactivatedKey);

        // Act
        var result = await _service.DeactivateKeyAsync(keyId);

        // Assert
        result.Should().NotBeNull();
        result!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DeactivateKeyAsync_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(It.IsAny<string>())).ReturnsAsync((LedgerKey?)null);

        // Act
        var result = await _service.DeactivateKeyAsync("nonexistent");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region DeleteKeyAsync Tests

    [Fact]
    public async Task DeleteKeyAsync_WithValidId_DeletesKey()
    {
        // Arrange
        var keyId = "test-id";
        _mockRepository.Setup(r => r.ExistsAsync(keyId)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.DeleteAsync(keyId)).ReturnsAsync(true);

        // Act
        var result = await _service.DeleteKeyAsync(keyId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteKeyAsync_WithNonExistentId_ReturnsFalse()
    {
        // Arrange
        _mockRepository.Setup(r => r.ExistsAsync(It.IsAny<string>())).ReturnsAsync(false);

        // Act
        var result = await _service.DeleteKeyAsync("nonexistent");

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
