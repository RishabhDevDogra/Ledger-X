using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;
using LedgerX.Models;
using LedgerX.Repositories;

namespace LedgerX.Tests.Repositories;

public class JournalEntryRepositoryTests
{
    private readonly JournalEntryRepository _repository;

    public JournalEntryRepositoryTests()
    {
        _repository = new JournalEntryRepository();
    }

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ReturnsSampleEntries()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(4);
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsEntry()
    {
        // Arrange
        var allEntries = await _repository.GetAllAsync();
        var entryId = allEntries.First().Id;

        // Act
        var result = await _repository.GetByIdAsync(entryId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(entryId);
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

    #region GetPostedEntriesAsync Tests

    [Fact]
    public async Task GetPostedEntriesAsync_ReturnsOnlyPostedEntries()
    {
        // Act
        var result = await _repository.GetPostedEntriesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().AllSatisfy(e => e.IsPosted.Should().BeTrue());
    }

    [Fact]
    public async Task GetPostedEntriesAsync_ContainsAtLeast3Entries()
    {
        // Act
        var result = await _repository.GetPostedEntriesAsync();

        // Assert
        result.Should().HaveCountGreaterThanOrEqualTo(3);
    }

    #endregion

    #region GetEntriesByDateRangeAsync Tests

    [Fact]
    public async Task GetEntriesByDateRangeAsync_WithValidRange_ReturnsEntries()
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 1, 15);

        // Act
        var result = await _repository.GetEntriesByDateRangeAsync(startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.Should().AllSatisfy(e => 
        {
            e.EntryDate.Should().BeOnOrAfter(startDate);
            e.EntryDate.Should().BeOnOrBefore(endDate.AddSeconds(1));
        });
    }

    [Fact]
    public async Task GetEntriesByDateRangeAsync_WithEmptyRange_ReturnsEmpty()
    {
        // Arrange
        var startDate = new DateTime(2030, 1, 1);
        var endDate = new DateTime(2030, 1, 31);

        // Act
        var result = await _repository.GetEntriesByDateRangeAsync(startDate, endDate);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_WithValidEntry_AddsAndReturns()
    {
        // Arrange
        var entry = new JournalEntry
        {
            Description = "Test Entry",
            ReferenceNumber = "JE-TEST",
            EntryDate = DateTime.UtcNow,
            Lines = new()
            {
                new JournalLine { AccountCode = "1000", DebitAmount = 100m },
                new JournalLine { AccountCode = "2000", CreditAmount = 100m }
            }
        };

        // Act
        var result = await _repository.AddAsync(entry);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeNullOrEmpty();
        var addedEntry = await _repository.GetByIdAsync(result.Id);
        addedEntry.Should().NotBeNull();
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithValidEntry_UpdatesAndReturns()
    {
        // Arrange
        var allEntries = await _repository.GetAllAsync();
        var entry = allEntries.First();
        var originalDescription = entry.Description;
        entry.Description = "Updated Description";

        // Act
        var result = await _repository.UpdateAsync(entry);

        // Assert
        result.Description.Should().Be("Updated Description");
        var updatedEntry = await _repository.GetByIdAsync(entry.Id);
        updatedEntry!.Description.Should().Be("Updated Description");
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WithValidId_Deletes()
    {
        // Arrange
        var newEntry = await _repository.AddAsync(new JournalEntry
        {
            Description = "To Delete",
            ReferenceNumber = "JE-DELETE",
            EntryDate = DateTime.UtcNow,
            Lines = new()
        });

        // Act
        var result = await _repository.DeleteAsync(newEntry.Id);

        // Assert
        result.Should().BeTrue();
        var deletedEntry = await _repository.GetByIdAsync(newEntry.Id);
        deletedEntry.Should().BeNull();
    }

    #endregion

    #region ExistsAsync Tests

    [Fact]
    public async Task ExistsAsync_WithValidId_ReturnsTrue()
    {
        // Arrange
        var allEntries = await _repository.GetAllAsync();
        var entryId = allEntries.First().Id;

        // Act
        var result = await _repository.ExistsAsync(entryId);

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
