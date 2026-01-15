using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using LedgerX.Controllers;
using LedgerX.DTOs;
using LedgerX.Services;
using LedgerX.Tests.Fixtures;

namespace LedgerX.Tests.Controllers;

public class LedgerKeysControllerTests
{
    private readonly Mock<ILedgerKeyService> _mockService;
    private readonly Mock<ILogger<LedgerKeysController>> _mockLogger;
    private readonly LedgerKeysController _controller;

    public LedgerKeysControllerTests()
    {
        _mockService = new Mock<ILedgerKeyService>();
        _mockLogger = new Mock<ILogger<LedgerKeysController>>();
        _controller = new LedgerKeysController(_mockService.Object, _mockLogger.Object);
    }

    #region GetAllKeys Tests

    [Fact]
    public async Task GetAllKeys_ReturnsOkWithKeys()
    {
        // Arrange
        var keys = new[] {
            new LedgerKeyDto { Id = "1", KeyName = "Key 1", IsActive = true, CreatedAt = DateTime.UtcNow },
            new LedgerKeyDto { Id = "2", KeyName = "Key 2", IsActive = true, CreatedAt = DateTime.UtcNow }
        };
        _mockService.Setup(s => s.GetAllKeysAsync()).ReturnsAsync(keys);

        // Act
        var result = await _controller.GetAllKeys();

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        var returnedKeys = okResult.Value as IEnumerable<LedgerKeyDto>;
        returnedKeys.Should().HaveCount(2);
    }

    #endregion

    #region GetKeyById Tests

    [Fact]
    public async Task GetKeyById_WithValidId_ReturnsOk()
    {
        // Arrange
        var key = new LedgerKeyDto { Id = "1", KeyName = "Key 1", IsActive = true, CreatedAt = DateTime.UtcNow };
        _mockService.Setup(s => s.GetKeyByIdAsync("1")).ReturnsAsync(key);

        // Act
        var result = await _controller.GetKeyById("1");

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
    }

    [Fact]
    public async Task GetKeyById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        _mockService.Setup(s => s.GetKeyByIdAsync(It.IsAny<string>())).ReturnsAsync((LedgerKeyDto?)null);

        // Act
        var result = await _controller.GetKeyById("invalid");

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region GetActiveKeys Tests

    [Fact]
    public async Task GetActiveKeys_ReturnsOnlyActiveKeys()
    {
        // Arrange
        var keys = new[] {
            new LedgerKeyDto { Id = "1", KeyName = "Active Key", IsActive = true, CreatedAt = DateTime.UtcNow }
        };
        _mockService.Setup(s => s.GetActiveKeysAsync()).ReturnsAsync(keys);

        // Act
        var result = await _controller.GetActiveKeys();

        // Assert
        var okResult = result.Result as OkObjectResult;
        var returnedKeys = okResult!.Value as IEnumerable<LedgerKeyDto>;
        returnedKeys.Should().AllSatisfy(k => k.IsActive.Should().BeTrue());
    }

    #endregion

    #region GetExpiredKeys Tests

    [Fact]
    public async Task GetExpiredKeys_ReturnsOnlyExpiredKeys()
    {
        // Arrange
        var keys = new[] {
            new LedgerKeyDto { Id = "1", KeyName = "Expired Key", IsActive = false, ExpiresAt = DateTime.UtcNow.AddDays(-1) }
        };
        _mockService.Setup(s => s.GetExpiredKeysAsync()).ReturnsAsync(keys);

        // Act
        var result = await _controller.GetExpiredKeys();

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
    }

    #endregion

    #region CreateKey Tests

    [Fact]
    public async Task CreateKey_WithValidRequest_ReturnsCreatedAtAction()
    {
        // Arrange
        var request = TestDataFixture.ValidLedgerKeyRequest;
        var createdKey = new LedgerKeyDto { Id = "1", KeyName = request.KeyName, IsActive = true, CreatedAt = DateTime.UtcNow };
        _mockService.Setup(s => s.CreateKeyAsync(request)).ReturnsAsync(createdKey);

        // Act
        var result = await _controller.CreateKey(request);

        // Assert
        var createdResult = result.Result as CreatedAtActionResult;
        createdResult.Should().NotBeNull();
        createdResult!.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task CreateKey_WithMissingKeyName_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateLedgerKeyDto { KeyName = "" };
        _mockService.Setup(s => s.CreateKeyAsync(request))
            .ThrowsAsync(new ArgumentException("Key name is required"));

        // Act
        var result = await _controller.CreateKey(request);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region RotateKey Tests

    [Fact]
    public async Task RotateKey_WithValidId_ReturnsOk()
    {
        // Arrange
        var key = new LedgerKeyDto { Id = "1", KeyName = "Key 1", IsActive = true, CreatedAt = DateTime.UtcNow };
        _mockService.Setup(s => s.RotateKeyAsync("1")).ReturnsAsync(key);

        // Act
        var result = await _controller.RotateKey("1");

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
    }

    [Fact]
    public async Task RotateKey_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        _mockService.Setup(s => s.RotateKeyAsync(It.IsAny<string>())).ReturnsAsync((LedgerKeyDto?)null);

        // Act
        var result = await _controller.RotateKey("invalid");

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region DeactivateKey Tests

    [Fact]
    public async Task DeactivateKey_WithValidId_ReturnsOk()
    {
        // Arrange
        var key = new LedgerKeyDto { Id = "1", KeyName = "Key 1", IsActive = false, CreatedAt = DateTime.UtcNow };
        _mockService.Setup(s => s.DeactivateKeyAsync("1")).ReturnsAsync(key);

        // Act
        var result = await _controller.DeactivateKey("1");

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        (okResult!.Value as LedgerKeyDto)!.IsActive.Should().BeFalse();
    }

    #endregion

    #region DeleteKey Tests

    [Fact]
    public async Task DeleteKey_WithValidId_ReturnsNoContent()
    {
        // Arrange
        _mockService.Setup(s => s.DeleteKeyAsync("1")).ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteKey("1");

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteKey_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        _mockService.Setup(s => s.DeleteKeyAsync(It.IsAny<string>())).ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteKey("invalid");

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion
}
