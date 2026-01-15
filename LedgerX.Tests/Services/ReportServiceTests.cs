using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using Xunit;
using LedgerX.Models;
using LedgerX.Repositories;
using LedgerX.Services;
using LedgerX.Tests.Fixtures;
using Microsoft.Extensions.Logging;

namespace LedgerX.Tests.Services;

public class ReportServiceTests
{
    private readonly Mock<IJournalEntryRepository> _mockJournalRepository;
    private readonly Mock<IAccountRepository> _mockAccountRepository;
    private readonly Mock<ILogger<ReportService>> _mockLogger;
    private readonly ReportService _service;

    public ReportServiceTests()
    {
        _mockJournalRepository = new Mock<IJournalEntryRepository>();
        _mockAccountRepository = new Mock<IAccountRepository>();
        _mockLogger = new Mock<ILogger<ReportService>>();
        _service = new ReportService(_mockJournalRepository.Object, _mockAccountRepository.Object, _mockLogger.Object);
    }

    #region GetTrialBalanceAsync Tests

    [Fact]
    public async Task GetTrialBalanceAsync_GeneratesCorrectTrialBalance()
    {
        // Arrange
        var accounts = TestDataFixture.SampleAccounts;
        var entries = new List<JournalEntry>
        {
            new()
            {
                Id = "1",
                IsPosted = true,
                ReferenceNumber = "JE-001",
                Description = "Test",
                EntryDate = DateTime.UtcNow,
                Lines = new()
                {
                    new JournalLine { AccountCode = "1000", DebitAmount = 1000m },
                    new JournalLine { AccountCode = "2000", CreditAmount = 1000m }
                }
            }
        };

        _mockAccountRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(accounts);
        _mockJournalRepository.Setup(r => r.GetPostedEntriesAsync()).ReturnsAsync(entries);

        // Act
        var result = await _service.GetTrialBalanceAsync();

        // Assert
        result.Should().NotBeNull();
        result.TotalDebits.Should().Be(1000m);
        result.TotalCredits.Should().Be(1000m);
        result.IsBalanced.Should().BeTrue();
    }

    [Fact]
    public async Task GetTrialBalanceAsync_WithImbalancedEntries_SetsIsBalancedToFalse()
    {
        // Arrange
        var accounts = TestDataFixture.SampleAccounts;
        var entries = new List<JournalEntry>
        {
            new()
            {
                Id = "1",
                IsPosted = true,
                ReferenceNumber = "JE-001",
                Description = "Test",
                EntryDate = DateTime.UtcNow,
                Lines = new()
                {
                    new JournalLine { AccountCode = "1000", DebitAmount = 1000m },
                    new JournalLine { AccountCode = "2000", CreditAmount = 500m }
                }
            }
        };

        _mockAccountRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(accounts);
        _mockJournalRepository.Setup(r => r.GetPostedEntriesAsync()).ReturnsAsync(entries);

        // Act
        var result = await _service.GetTrialBalanceAsync();

        // Assert
        result.IsBalanced.Should().BeFalse();
    }

    [Fact]
    public async Task GetTrialBalanceAsync_WithNoEntries_ReturnsZeroTotals()
    {
        // Arrange
        var accounts = TestDataFixture.SampleAccounts;
        _mockAccountRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(accounts);
        _mockJournalRepository.Setup(r => r.GetPostedEntriesAsync()).ReturnsAsync(new List<JournalEntry>());

        // Act
        var result = await _service.GetTrialBalanceAsync();

        // Assert
        result.TotalDebits.Should().Be(0m);
        result.TotalCredits.Should().Be(0m);
        result.IsBalanced.Should().BeTrue();
    }

    [Fact]
    public async Task GetTrialBalanceAsync_IncludesAllAccounts()
    {
        // Arrange
        var accounts = TestDataFixture.SampleAccounts;
        _mockAccountRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(accounts);
        _mockJournalRepository.Setup(r => r.GetPostedEntriesAsync()).ReturnsAsync(new List<JournalEntry>());

        // Act
        var result = await _service.GetTrialBalanceAsync();

        // Assert
        result.Accounts.Should().HaveCount(3);
    }

    #endregion

    #region GetTotalDebitAsync Tests

    [Fact]
    public async Task GetTotalDebitAsync_CalculatesCorrectTotal()
    {
        // Arrange
        var entries = new List<JournalEntry>
        {
            new()
            {
                Id = "1",
                IsPosted = true,
                ReferenceNumber = "JE-001",
                Description = "Test",
                EntryDate = DateTime.UtcNow,
                Lines = new()
                {
                    new JournalLine { AccountCode = "1000", DebitAmount = 1000m },
                    new JournalLine { AccountCode = "2000", DebitAmount = 500m }
                }
            },
            new()
            {
                Id = "2",
                IsPosted = true,
                ReferenceNumber = "JE-002",
                Description = "Test",
                EntryDate = DateTime.UtcNow,
                Lines = new()
                {
                    new JournalLine { AccountCode = "1000", DebitAmount = 1500m }
                }
            }
        };

        _mockJournalRepository.Setup(r => r.GetPostedEntriesAsync()).ReturnsAsync(entries);

        // Act
        var result = await _service.GetTotalDebitAsync();

        // Assert
        result.Should().Be(3000m);
    }

    [Fact]
    public async Task GetTotalDebitAsync_WithNoEntries_ReturnsZero()
    {
        // Arrange
        _mockJournalRepository.Setup(r => r.GetPostedEntriesAsync()).ReturnsAsync(new List<JournalEntry>());

        // Act
        var result = await _service.GetTotalDebitAsync();

        // Assert
        result.Should().Be(0m);
    }

    #endregion

    #region GetTotalCreditAsync Tests

    [Fact]
    public async Task GetTotalCreditAsync_CalculatesCorrectTotal()
    {
        // Arrange
        var entries = new List<JournalEntry>
        {
            new()
            {
                Id = "1",
                IsPosted = true,
                ReferenceNumber = "JE-001",
                Description = "Test",
                EntryDate = DateTime.UtcNow,
                Lines = new()
                {
                    new JournalLine { AccountCode = "1000", CreditAmount = 1000m },
                    new JournalLine { AccountCode = "2000", CreditAmount = 500m }
                }
            }
        };

        _mockJournalRepository.Setup(r => r.GetPostedEntriesAsync()).ReturnsAsync(entries);

        // Act
        var result = await _service.GetTotalCreditAsync();

        // Assert
        result.Should().Be(1500m);
    }

    [Fact]
    public async Task GetTotalCreditAsync_WithNoEntries_ReturnsZero()
    {
        // Arrange
        _mockJournalRepository.Setup(r => r.GetPostedEntriesAsync()).ReturnsAsync(new List<JournalEntry>());

        // Act
        var result = await _service.GetTotalCreditAsync();

        // Assert
        result.Should().Be(0m);
    }

    #endregion
}
