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

public class AccountServiceTests
{
    private readonly Mock<IAccountRepository> _mockRepository;
    private readonly Mock<ILogger<AccountService>> _mockLogger;
    private readonly AccountService _service;

    public AccountServiceTests()
    {
        _mockRepository = new Mock<IAccountRepository>();
        _mockLogger = new Mock<ILogger<AccountService>>();
        _service = new AccountService(_mockRepository.Object, _mockLogger.Object);
    }

    #region GetAllAccountsAsync Tests

    [Fact]
    public async Task GetAllAccountsAsync_ReturnsAllAccounts()
    {
        // Arrange
        var accounts = TestDataFixture.SampleAccounts;
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(accounts);

        // Act
        var result = await _service.GetAllAccountsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllAccountsAsync_WhenNoAccounts_ReturnsEmptyList()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Account>());

        // Act
        var result = await _service.GetAllAccountsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetAccountByIdAsync Tests

    [Fact]
    public async Task GetAccountByIdAsync_WithValidId_ReturnsAccount()
    {
        // Arrange
        var account = TestDataFixture.SampleAccount;
        _mockRepository.Setup(r => r.GetByIdAsync(account.Id)).ReturnsAsync(account);

        // Act
        var result = await _service.GetAccountByIdAsync(account.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(account.Id);
        result.Name.Should().Be(account.Name);
    }

    [Fact]
    public async Task GetAccountByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(It.IsAny<string>())).ReturnsAsync((Account?)null);

        // Act
        var result = await _service.GetAccountByIdAsync("invalid-id");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAccountByIdAsync_WithNullId_ReturnsNull()
    {
        // Act
        var result = await _service.GetAccountByIdAsync(null!);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetAccountByCodeAsync Tests

    [Fact]
    public async Task GetAccountByCodeAsync_WithValidCode_ReturnsAccount()
    {
        // Arrange
        var account = TestDataFixture.SampleAccount;
        _mockRepository.Setup(r => r.GetByCodeAsync(account.Code)).ReturnsAsync(account);

        // Act
        var result = await _service.GetAccountByCodeAsync(account.Code);

        // Assert
        result.Should().NotBeNull();
        result!.Code.Should().Be(account.Code);
    }

    [Fact]
    public async Task GetAccountByCodeAsync_WithInvalidCode_ReturnsNull()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByCodeAsync(It.IsAny<string>())).ReturnsAsync((Account?)null);

        // Act
        var result = await _service.GetAccountByCodeAsync("9999");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetActiveAccountsAsync Tests

    [Fact]
    public async Task GetActiveAccountsAsync_ReturnsOnlyActiveAccounts()
    {
        // Arrange
        var activeAccounts = new List<Account>
        {
            new() { Id = "1", Code = "1000", Name = "Cash", Type = "Asset", IsActive = true },
            new() { Id = "2", Code = "2000", Name = "Payable", Type = "Liability", IsActive = true }
        };
        _mockRepository.Setup(r => r.GetActiveAccountsAsync()).ReturnsAsync(activeAccounts);

        // Act
        var result = await _service.GetActiveAccountsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(a => a.IsActive.Should().BeTrue());
    }

    #endregion

    #region CreateAccountAsync Tests

    [Fact]
    public async Task CreateAccountAsync_WithValidRequest_CreatesAccount()
    {
        // Arrange
        var request = TestDataFixture.ValidAccountRequest;
        var createdAccount = new Account
        {
            Id = Guid.NewGuid().ToString(),
            Code = request.Code,
            Name = request.Name,
            Type = request.Type,
            Balance = request.Balance
        };
        _mockRepository.Setup(r => r.GetByCodeAsync(request.Code)).ReturnsAsync((Account?)null);
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Account>())).ReturnsAsync(createdAccount);

        // Act
        var result = await _service.CreateAccountAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Code.Should().Be(request.Code);
        result.Name.Should().Be(request.Name);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Account>()), Times.Once);
    }

    [Fact]
    public async Task CreateAccountAsync_WithDuplicateCode_ThrowsException()
    {
        // Arrange
        var request = TestDataFixture.ValidAccountRequest;
        _mockRepository.Setup(r => r.GetByCodeAsync(request.Code))
            .ReturnsAsync(new Account { Code = request.Code });

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAccountAsync(request));
    }

    [Fact]
    public async Task CreateAccountAsync_WithMissingCode_ThrowsException()
    {
        // Arrange
        var request = new CreateAccountDto { Code = "", Name = "Test", Type = "Asset" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAccountAsync(request));
    }

    [Fact]
    public async Task CreateAccountAsync_WithMissingName_ThrowsException()
    {
        // Arrange
        var request = new CreateAccountDto { Code = "1000", Name = "", Type = "Asset" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAccountAsync(request));
    }

    [Fact]
    public async Task CreateAccountAsync_WithInvalidType_ThrowsException()
    {
        // Arrange
        var request = new CreateAccountDto { Code = "1000", Name = "Test", Type = "InvalidType" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAccountAsync(request));
    }

    #endregion

    #region UpdateAccountAsync Tests

    [Fact]
    public async Task UpdateAccountAsync_WithValidData_UpdatesAccount()
    {
        // Arrange
        var accountId = "test-id";
        var account = new Account { Id = accountId, Name = "Old Name", Type = "Asset" };
        var updateRequest = new UpdateAccountDto { Name = "New Name" };
        _mockRepository.Setup(r => r.GetByIdAsync(accountId)).ReturnsAsync(account);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Account>())).ReturnsAsync(account);

        // Act
        var result = await _service.UpdateAccountAsync(accountId, updateRequest);

        // Assert
        result.Should().NotBeNull();
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Account>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAccountAsync_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        var updateRequest = new UpdateAccountDto { Name = "New Name" };
        _mockRepository.Setup(r => r.GetByIdAsync(It.IsAny<string>())).ReturnsAsync((Account?)null);

        // Act
        var result = await _service.UpdateAccountAsync("nonexistent", updateRequest);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region DeleteAccountAsync Tests

    [Fact]
    public async Task DeleteAccountAsync_WithValidId_DeletesAccount()
    {
        // Arrange
        var accountId = "test-id";
        _mockRepository.Setup(r => r.ExistsAsync(accountId)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.DeleteAsync(accountId)).ReturnsAsync(true);

        // Act
        var result = await _service.DeleteAccountAsync(accountId);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(r => r.DeleteAsync(accountId), Times.Once);
    }

    [Fact]
    public async Task DeleteAccountAsync_WithNonExistentId_ReturnsFalse()
    {
        // Arrange
        _mockRepository.Setup(r => r.ExistsAsync(It.IsAny<string>())).ReturnsAsync(false);

        // Act
        var result = await _service.DeleteAccountAsync("nonexistent");

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
