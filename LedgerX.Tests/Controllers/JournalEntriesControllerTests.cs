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

public class JournalEntriesControllerTests
{
    private readonly Mock<IJournalEntryService> _mockService;
    private readonly Mock<ILogger<JournalEntriesController>> _mockLogger;
    private readonly JournalEntriesController _controller;

    public JournalEntriesControllerTests()
    {
        _mockService = new Mock<IJournalEntryService>();
        _mockLogger = new Mock<ILogger<JournalEntriesController>>();
        _controller = new JournalEntriesController(_mockService.Object, _mockLogger.Object);
    }

    #region GetAllEntries Tests

    [Fact]
    public async Task GetAllEntries_ReturnsOkWithEntries()
    {
        // Arrange
        var entries = new[] { 
            new JournalEntryDto { Id = "1", Description = "Entry 1", ReferenceNumber = "JE-001", IsPosted = true, Lines = new() },
            new JournalEntryDto { Id = "2", Description = "Entry 2", ReferenceNumber = "JE-002", IsPosted = false, Lines = new() }
        };
        _mockService.Setup(s => s.GetAllEntriesAsync()).ReturnsAsync(entries);

        // Act
        var result = await _controller.GetAllEntries();

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        var returnedEntries = okResult.Value as IEnumerable<JournalEntryDto>;
        returnedEntries.Should().HaveCount(2);
    }

    #endregion

    #region GetEntryById Tests

    [Fact]
    public async Task GetEntryById_WithValidId_ReturnsOk()
    {
        // Arrange
        var entry = new JournalEntryDto { Id = "1", Description = "Entry 1", ReferenceNumber = "JE-001", IsPosted = true, Lines = new() };
        _mockService.Setup(s => s.GetEntryByIdAsync("1")).ReturnsAsync(entry);

        // Act
        var result = await _controller.GetEntryById("1");

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
    }

    [Fact]
    public async Task GetEntryById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        _mockService.Setup(s => s.GetEntryByIdAsync(It.IsAny<string>())).ReturnsAsync((JournalEntryDto?)null);

        // Act
        var result = await _controller.GetEntryById("invalid");

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region GetPostedEntries Tests

    [Fact]
    public async Task GetPostedEntries_ReturnsOnlyPostedEntries()
    {
        // Arrange
        var entries = new[] { 
            new JournalEntryDto { Id = "1", IsPosted = true, Description = "Entry 1", ReferenceNumber = "JE-001", Lines = new() }
        };
        _mockService.Setup(s => s.GetPostedEntriesAsync()).ReturnsAsync(entries);

        // Act
        var result = await _controller.GetPostedEntries();

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        var returnedEntries = okResult!.Value as IEnumerable<JournalEntryDto>;
        returnedEntries.Should().AllSatisfy(e => e.IsPosted.Should().BeTrue());
    }

    #endregion

    #region GetEntriesByDateRange Tests

    [Fact]
    public async Task GetEntriesByDateRange_WithValidDates_ReturnsOk()
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 1, 31);
        var entries = new[] { 
            new JournalEntryDto { Id = "1", Description = "Entry 1", ReferenceNumber = "JE-001", EntryDate = new DateTime(2024, 1, 15), Lines = new() }
        };
        _mockService.Setup(s => s.GetEntriesByDateRangeAsync(startDate, endDate)).ReturnsAsync(entries);

        // Act
        var result = await _controller.GetEntriesByDateRange(startDate, endDate);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
    }

    [Fact]
    public async Task GetEntriesByDateRange_WithInvalidDateRange_ReturnsBadRequest()
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 31);
        var endDate = new DateTime(2024, 1, 1);
        _mockService.Setup(s => s.GetEntriesByDateRangeAsync(startDate, endDate))
            .ThrowsAsync(new ArgumentException("Invalid date range"));

        // Act
        var result = await _controller.GetEntriesByDateRange(startDate, endDate);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region CreateJournalEntry Tests

    [Fact]
    public async Task CreateJournalEntry_WithValidRequest_ReturnsCreatedAtAction()
    {
        // Arrange
        var request = TestDataFixture.ValidJournalEntryRequest;
        var createdEntry = new JournalEntryDto { Id = "1", Description = request.Description, ReferenceNumber = request.ReferenceNumber, IsPosted = false, Lines = new() };
        _mockService.Setup(s => s.CreateJournalEntryAsync(request)).ReturnsAsync(createdEntry);

        // Act
        var result = await _controller.CreateJournalEntry(request);

        // Assert
        var createdResult = result.Result as CreatedAtActionResult;
        createdResult.Should().NotBeNull();
        createdResult!.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task CreateJournalEntry_WithImbalancedEntry_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateJournalEntryDto
        {
            Description = "Imbalanced",
            ReferenceNumber = "JE-001",
            EntryDate = DateTime.UtcNow,
            Lines = new() { new JournalLineDto { AccountCode = "1000", AccountId = "1", DebitAmount = 1000m } }
        };
        _mockService.Setup(s => s.CreateJournalEntryAsync(request))
            .ThrowsAsync(new ArgumentException("Journal entry must have at least 2 lines"));

        // Act
        var result = await _controller.CreateJournalEntry(request);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region UpdateJournalEntry Tests

    [Fact]
    public async Task UpdateJournalEntry_WithValidId_ReturnsOk()
    {
        // Arrange
        var updateRequest = new UpdateJournalEntryDto { Description = "Updated" };
        var updatedEntry = new JournalEntryDto { Id = "1", Description = "Updated", ReferenceNumber = "JE-001", IsPosted = false, Lines = new() };
        _mockService.Setup(s => s.UpdateJournalEntryAsync("1", updateRequest)).ReturnsAsync(updatedEntry);

        // Act
        var result = await _controller.UpdateJournalEntry("1", updateRequest);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateJournalEntry_WithPostedEntry_ReturnsBadRequest()
    {
        // Arrange
        var updateRequest = new UpdateJournalEntryDto { Description = "Updated" };
        _mockService.Setup(s => s.UpdateJournalEntryAsync("1", updateRequest))
            .ThrowsAsync(new InvalidOperationException("Cannot update a posted journal entry"));

        // Act
        var result = await _controller.UpdateJournalEntry("1", updateRequest);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region PostJournalEntry Tests

    [Fact]
    public async Task PostJournalEntry_WithValidId_ReturnsNoContent()
    {
        // Arrange
        _mockService.Setup(s => s.PostJournalEntryAsync("1")).ReturnsAsync(true);

        // Act
        var result = await _controller.PostJournalEntry("1");

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task PostJournalEntry_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        _mockService.Setup(s => s.PostJournalEntryAsync(It.IsAny<string>())).ReturnsAsync(false);

        // Act
        var result = await _controller.PostJournalEntry("invalid");

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region DeleteJournalEntry Tests

    [Fact]
    public async Task DeleteJournalEntry_WithUnpostedEntry_ReturnsNoContent()
    {
        // Arrange
        _mockService.Setup(s => s.DeleteJournalEntryAsync("1")).ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteJournalEntry("1");

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteJournalEntry_WithPostedEntry_ReturnsBadRequest()
    {
        // Arrange
        _mockService.Setup(s => s.DeleteJournalEntryAsync("1"))
            .ThrowsAsync(new InvalidOperationException("Cannot delete a posted journal entry"));

        // Act
        var result = await _controller.DeleteJournalEntry("1");

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion
}
