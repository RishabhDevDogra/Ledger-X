using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;
using LedgerX.Models;
using LedgerX.Repositories;

namespace LedgerX.Tests.Repositories;

public class AccountRepositoryTests
{
    private readonly AccountRepository _repository;

    public AccountRepositoryTests()
    {
        _repository = new AccountRepository();
    }

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ReturnsSampleAccounts()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(10);
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsAccount()
    {
        // Arrange
        var allAccounts = await _repository.GetAllAsync();
        var accountId = allAccounts.First().Id;

        // Act
        var result = await _repository.GetByIdAsync(accountId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(accountId);
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

    #region GetByCodeAsync Tests

    [Fact]
    public async Task GetByCodeAsync_WithValidCode_ReturnsAccount()
    {
        // Act
        var result = await _repository.GetByCodeAsync("10");

        // Assert
        result.Should().NotBeNull();
        result!.Code.Should().Be("10");
    }

    [Fact]
    public async Task GetByCodeAsync_WithInvalidCode_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByCodeAsync("9999");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetActiveAccountsAsync Tests

    [Fact]
    public async Task GetActiveAccountsAsync_ReturnsOnlyActiveAccounts()
    {
        // Act
        var result = await _repository.GetActiveAccountsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().AllSatisfy(a => a.IsActive.Should().BeTrue());
    }

    #endregion

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_WithValidAccount_AddsAndReturns()
    {
        // Arrange
        var account = new Account
        {
            Code = "9000",
            Name = "Test Account",
            Type = "Asset",
            Balance = 5000m
        };

        // Act
        var result = await _repository.AddAsync(account);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeNullOrEmpty();
        var addedAccount = await _repository.GetByIdAsync(result.Id);
        addedAccount.Should().NotBeNull();
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithValidAccount_UpdatesAndReturns()
    {
        // Arrange
        var allAccounts = await _repository.GetAllAsync();
        var account = allAccounts.First();
        var originalName = account.Name;
        account.Name = "Updated Name";

        // Act
        var result = await _repository.UpdateAsync(account);

        // Assert
        result.Name.Should().Be("Updated Name");
        var updatedAccount = await _repository.GetByIdAsync(account.Id);
        updatedAccount!.Name.Should().Be("Updated Name");
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WithValidId_Deletes()
    {
        // Arrange
        var newAccount = await _repository.AddAsync(new Account
        {
            Code = "8000",
            Name = "To Delete",
            Type = "Asset"
        });

        // Act
        var result = await _repository.DeleteAsync(newAccount.Id);

        // Assert
        result.Should().BeTrue();
        var deletedAccount = await _repository.GetByIdAsync(newAccount.Id);
        deletedAccount.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithInvalidId_ReturnsFalse()
    {
        // Act
        var result = await _repository.DeleteAsync("invalid-id");

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region ExistsAsync Tests

    [Fact]
    public async Task ExistsAsync_WithValidId_ReturnsTrue()
    {
        // Arrange
        var allAccounts = await _repository.GetAllAsync();
        var accountId = allAccounts.First().Id;

        // Act
        var result = await _repository.ExistsAsync(accountId);

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
