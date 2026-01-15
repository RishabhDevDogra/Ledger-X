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

public class JournalEntryServiceTests
{
    private readonly Mock<IJournalEntryRepository> _mockJournalRepository;
    private readonly Mock<IAccountRepository> _mockAccountRepository;
    private readonly Mock<ILogger<JournalEntryService>> _mockLogger;
    private readonly JournalEntryService _service;

    public JournalEntryServiceTests()
    {
        _mockJournalRepository = new Mock<IJournalEntryRepository>();
        _mockAccountRepository = new Mock<IAccountRepository>();
        _mockLogger = new Mock<ILogger<JournalEntryService>>();
        _service = new JournalEntryService(_mockJournalRepository.Object, _mockAccountRepository.Object, _mockLogger.Object);
    }

    #region GetAllEntriesAsync Tests

    [Fact]
    public async Task GetAllEntriesAsync_ReturnsAllEntries()
    {
        // Arrange
        var entries = TestDataFixture.SampleJournalEntries;
        _mockJournalRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(entries);

        // Act
        var result = await _service.GetAllEntriesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllEntriesAsync_WhenNoEntries_ReturnsEmptyList()
    {
        // Arrange
        _mockJournalRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<JournalEntry>());

        // Act
        var result = await _service.GetAllEntriesAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetEntryByIdAsync Tests

    [Fact]
    public async Task GetEntryByIdAsync_WithValidId_ReturnsEntry()
    {
        // Arrange
        var entry = TestDataFixture.SampleJournalEntry;
        _mockJournalRepository.Setup(r => r.GetByIdAsync(entry.Id)).ReturnsAsync(entry);

        // Act
        var result = await _service.GetEntryByIdAsync(entry.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(entry.Id);
    }

    [Fact]
    public async Task GetEntryByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        _mockJournalRepository.Setup(r => r.GetByIdAsync(It.IsAny<string>())).ReturnsAsync((JournalEntry?)null);

        // Act
        var result = await _service.GetEntryByIdAsync("invalid-id");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetPostedEntriesAsync Tests

    [Fact]
    public async Task GetPostedEntriesAsync_ReturnsOnlyPostedEntries()
    {
        // Arrange
        var postedEntries = new List<JournalEntry>
        {
            new() { Id = "1", IsPosted = true, Description = "Entry 1", ReferenceNumber = "JE-001", EntryDate = DateTime.UtcNow, Lines = new() },
            new() { Id = "2", IsPosted = true, Description = "Entry 2", ReferenceNumber = "JE-002", EntryDate = DateTime.UtcNow, Lines = new() }
        };
        _mockJournalRepository.Setup(r => r.GetPostedEntriesAsync()).ReturnsAsync(postedEntries);

        // Act
        var result = await _service.GetPostedEntriesAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(e => e.IsPosted.Should().BeTrue());
    }

    #endregion

    #region GetEntriesByDateRangeAsync Tests

    [Fact]
    public async Task GetEntriesByDateRangeAsync_WithValidDates_ReturnsEntriesInRange()
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 1, 10);
        var entries = TestDataFixture.SampleJournalEntries.Take(2).ToList();
        _mockJournalRepository.Setup(r => r.GetEntriesByDateRangeAsync(startDate, endDate)).ReturnsAsync(entries);

        // Act
        var result = await _service.GetEntriesByDateRangeAsync(startDate, endDate);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetEntriesByDateRangeAsync_WithStartAfterEnd_ThrowsException()
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 10);
        var endDate = new DateTime(2024, 1, 1);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetEntriesByDateRangeAsync(startDate, endDate));
    }

    #endregion

    #region CreateJournalEntryAsync Tests

    [Fact]
    public async Task CreateJournalEntryAsync_WithValidRequest_CreatesEntry()
    {
        // Arrange
        var request = TestDataFixture.ValidJournalEntryRequest;
        var createdEntry = new JournalEntry
        {
            Id = Guid.NewGuid().ToString(),
            Description = request.Description,
            ReferenceNumber = request.ReferenceNumber,
            EntryDate = request.EntryDate,
            IsPosted = false,
            Lines = new()
        };
        _mockJournalRepository.Setup(r => r.AddAsync(It.IsAny<JournalEntry>())).ReturnsAsync(createdEntry);

        // Act
        var result = await _service.CreateJournalEntryAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Description.Should().Be(request.Description);
        result.IsPosted.Should().BeFalse();
    }

    [Fact]
    public async Task CreateJournalEntryAsync_WithImbalancedEntry_ThrowsException()
    {
        // Arrange
        var request = new CreateJournalEntryDto
        {
            Description = "Imbalanced",
            ReferenceNumber = "JE-001",
            EntryDate = DateTime.UtcNow,
            Lines = new()
            {
                new JournalLineDto { AccountCode = "1000", AccountId = "1", DebitAmount = 1000m },
                new JournalLineDto { AccountCode = "2000", AccountId = "2", CreditAmount = 500m } // Imbalanced
            }
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateJournalEntryAsync(request));
    }

    [Fact]
    public async Task CreateJournalEntryAsync_WithLessThanTwoLines_ThrowsException()
    {
        // Arrange
        var request = new CreateJournalEntryDto
        {
            Description = "Single Line",
            ReferenceNumber = "JE-001",
            EntryDate = DateTime.UtcNow,
            Lines = new() { new JournalLineDto { AccountCode = "1000", AccountId = "1", DebitAmount = 1000m } }
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateJournalEntryAsync(request));
    }

    [Fact]
    public async Task CreateJournalEntryAsync_WithMissingDescription_ThrowsException()
    {
        // Arrange
        var request = new CreateJournalEntryDto
        {
            Description = "",
            ReferenceNumber = "JE-001",
            EntryDate = DateTime.UtcNow,
            Lines = new()
            {
                new JournalLineDto { AccountCode = "1000", AccountId = "1", DebitAmount = 1000m },
                new JournalLineDto { AccountCode = "2000", AccountId = "2", CreditAmount = 1000m }
            }
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateJournalEntryAsync(request));
    }

    [Fact]
    public async Task CreateJournalEntryAsync_WithBothDebitAndCredit_ThrowsException()
    {
        // Arrange
        var request = new CreateJournalEntryDto
        {
            Description = "Invalid",
            ReferenceNumber = "JE-001",
            EntryDate = DateTime.UtcNow,
            Lines = new()
            {
                new JournalLineDto { AccountCode = "1000", AccountId = "1", DebitAmount = 500m, CreditAmount = 500m },
                new JournalLineDto { AccountCode = "2000", AccountId = "2", DebitAmount = 1000m, CreditAmount = 0m }
            }
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateJournalEntryAsync(request));
    }

    #endregion

    #region UpdateJournalEntryAsync Tests

    [Fact]
    public async Task UpdateJournalEntryAsync_WithValidData_UpdatesEntry()
    {
        // Arrange
        var entryId = "test-id";
        var entry = new JournalEntry { Id = entryId, Description = "Old", IsPosted = false, ReferenceNumber = "JE-001", EntryDate = DateTime.UtcNow, Lines = new() };
        var updateRequest = new UpdateJournalEntryDto { Description = "New" };
        _mockJournalRepository.Setup(r => r.GetByIdAsync(entryId)).ReturnsAsync(entry);
        _mockJournalRepository.Setup(r => r.UpdateAsync(It.IsAny<JournalEntry>())).ReturnsAsync(entry);

        // Act
        var result = await _service.UpdateJournalEntryAsync(entryId, updateRequest);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateJournalEntryAsync_WithPostedEntry_ThrowsException()
    {
        // Arrange
        var entryId = "test-id";
        var entry = new JournalEntry { Id = entryId, IsPosted = true, ReferenceNumber = "JE-001", EntryDate = DateTime.UtcNow, Lines = new() };
        var updateRequest = new UpdateJournalEntryDto { Description = "New" };
        _mockJournalRepository.Setup(r => r.GetByIdAsync(entryId)).ReturnsAsync(entry);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateJournalEntryAsync(entryId, updateRequest));
    }

    #endregion

    #region PostJournalEntryAsync Tests

    [Fact]
    public async Task PostJournalEntryAsync_WithValidEntry_PostsEntry()
    {
        // Arrange
        var entryId = "test-id";
        var entry = new JournalEntry { Id = entryId, IsPosted = false, ReferenceNumber = "JE-001", EntryDate = DateTime.UtcNow, Lines = new() };
        _mockJournalRepository.Setup(r => r.GetByIdAsync(entryId)).ReturnsAsync(entry);
        _mockJournalRepository.Setup(r => r.UpdateAsync(It.IsAny<JournalEntry>())).ReturnsAsync(entry);

        // Act
        var result = await _service.PostJournalEntryAsync(entryId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task PostJournalEntryAsync_WithAlreadyPostedEntry_ReturnsFalse()
    {
        // Arrange
        var entryId = "test-id";
        var entry = new JournalEntry { Id = entryId, IsPosted = true, ReferenceNumber = "JE-001", EntryDate = DateTime.UtcNow, Lines = new() };
        _mockJournalRepository.Setup(r => r.GetByIdAsync(entryId)).ReturnsAsync(entry);

        // Act
        var result = await _service.PostJournalEntryAsync(entryId);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region DeleteJournalEntryAsync Tests

    [Fact]
    public async Task DeleteJournalEntryAsync_WithUnpostedEntry_DeletesEntry()
    {
        // Arrange
        var entryId = "test-id";
        var entry = new JournalEntry { Id = entryId, IsPosted = false, ReferenceNumber = "JE-001", EntryDate = DateTime.UtcNow, Lines = new() };
        _mockJournalRepository.Setup(r => r.GetByIdAsync(entryId)).ReturnsAsync(entry);
        _mockJournalRepository.Setup(r => r.DeleteAsync(entryId)).ReturnsAsync(true);

        // Act
        var result = await _service.DeleteJournalEntryAsync(entryId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteJournalEntryAsync_WithPostedEntry_ThrowsException()
    {
        // Arrange
        var entryId = "test-id";
        var entry = new JournalEntry { Id = entryId, IsPosted = true, ReferenceNumber = "JE-001", EntryDate = DateTime.UtcNow, Lines = new() };
        _mockJournalRepository.Setup(r => r.GetByIdAsync(entryId)).ReturnsAsync(entry);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DeleteJournalEntryAsync(entryId));
    }

    #endregion
}
