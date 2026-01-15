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

public class AccountsControllerTests
{
    private readonly Mock<IAccountService> _mockService;
    private readonly Mock<ILogger<AccountsController>> _mockLogger;
    private readonly AccountsController _controller;

    public AccountsControllerTests()
    {
        _mockService = new Mock<IAccountService>();
        _mockLogger = new Mock<ILogger<AccountsController>>();
        _controller = new AccountsController(_mockService.Object, _mockLogger.Object);
    }

    #region GetAllAccounts Tests

    [Fact]
    public async Task GetAllAccounts_ReturnsOkWithAccounts()
    {
        // Arrange
        var accounts = new[] { 
            new AccountDto { Id = "1", Code = "1000", Name = "Cash", Type = "Asset" },
            new AccountDto { Id = "2", Code = "2000", Name = "Payable", Type = "Liability" }
        };
        _mockService.Setup(s => s.GetAllAccountsAsync()).ReturnsAsync(accounts);

        // Act
        var result = await _controller.GetAllAccounts();

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        var returnedAccounts = okResult.Value as IEnumerable<AccountDto>;
        returnedAccounts.Should().HaveCount(2);
    }

    #endregion

    #region GetAccountById Tests

    [Fact]
    public async Task GetAccountById_WithValidId_ReturnsOkWithAccount()
    {
        // Arrange
        var account = new AccountDto { Id = "1", Code = "1000", Name = "Cash", Type = "Asset" };
        _mockService.Setup(s => s.GetAccountByIdAsync("1")).ReturnsAsync(account);

        // Act
        var result = await _controller.GetAccountById("1");

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GetAccountById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        _mockService.Setup(s => s.GetAccountByIdAsync(It.IsAny<string>())).ReturnsAsync((AccountDto?)null);

        // Act
        var result = await _controller.GetAccountById("invalid");

        // Assert
        var notFoundResult = result.Result as NotFoundObjectResult;
        notFoundResult.Should().NotBeNull();
        notFoundResult!.StatusCode.Should().Be(404);
    }

    #endregion

    #region GetAccountByCode Tests

    [Fact]
    public async Task GetAccountByCode_WithValidCode_ReturnsOk()
    {
        // Arrange
        var account = new AccountDto { Id = "1", Code = "1000", Name = "Cash", Type = "Asset" };
        _mockService.Setup(s => s.GetAccountByCodeAsync("1000")).ReturnsAsync(account);

        // Act
        var result = await _controller.GetAccountByCode("1000");

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
    }

    #endregion

    #region CreateAccount Tests

    [Fact]
    public async Task CreateAccount_WithValidRequest_ReturnsCreatedAtAction()
    {
        // Arrange
        var request = TestDataFixture.ValidAccountRequest;
        var createdAccount = new AccountDto { Id = "1", Code = request.Code, Name = request.Name, Type = request.Type };
        _mockService.Setup(s => s.CreateAccountAsync(request)).ReturnsAsync(createdAccount);

        // Act
        var result = await _controller.CreateAccount(request);

        // Assert
        var createdResult = result.Result as CreatedAtActionResult;
        createdResult.Should().NotBeNull();
        createdResult!.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task CreateAccount_WithValidationError_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateAccountDto { Code = "", Name = "Test", Type = "Asset" };
        _mockService.Setup(s => s.CreateAccountAsync(request)).ThrowsAsync(new ArgumentException("Code is required"));

        // Act
        var result = await _controller.CreateAccount(request);

        // Assert
        var badRequestResult = result.Result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult!.StatusCode.Should().Be(400);
    }

    #endregion

    #region UpdateAccount Tests

    [Fact]
    public async Task UpdateAccount_WithValidId_ReturnsOk()
    {
        // Arrange
        var updateRequest = new UpdateAccountDto { Name = "Updated Name" };
        var updatedAccount = new AccountDto { Id = "1", Code = "1000", Name = "Updated Name", Type = "Asset" };
        _mockService.Setup(s => s.UpdateAccountAsync("1", updateRequest)).ReturnsAsync(updatedAccount);

        // Act
        var result = await _controller.UpdateAccount("1", updateRequest);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAccount_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var updateRequest = new UpdateAccountDto { Name = "Updated Name" };
        _mockService.Setup(s => s.UpdateAccountAsync(It.IsAny<string>(), updateRequest)).ReturnsAsync((AccountDto?)null);

        // Act
        var result = await _controller.UpdateAccount("invalid", updateRequest);

        // Assert
        var notFoundResult = result.Result as NotFoundObjectResult;
        notFoundResult.Should().NotBeNull();
    }

    #endregion

    #region DeleteAccount Tests

    [Fact]
    public async Task DeleteAccount_WithValidId_ReturnsNoContent()
    {
        // Arrange
        _mockService.Setup(s => s.DeleteAccountAsync("1")).ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteAccount("1");

        // Assert
        result.Should().BeOfType<NoContentResult>();
        (result as NoContentResult)!.StatusCode.Should().Be(204);
    }

    [Fact]
    public async Task DeleteAccount_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        _mockService.Setup(s => s.DeleteAccountAsync(It.IsAny<string>())).ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteAccount("invalid");

        // Assert
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult.Should().NotBeNull();
    }

    #endregion
}
