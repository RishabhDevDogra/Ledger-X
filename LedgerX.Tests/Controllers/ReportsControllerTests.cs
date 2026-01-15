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

namespace LedgerX.Tests.Controllers;

public class ReportsControllerTests
{
    private readonly Mock<IReportService> _mockService;
    private readonly Mock<ILogger<ReportsController>> _mockLogger;
    private readonly ReportsController _controller;

    public ReportsControllerTests()
    {
        _mockService = new Mock<IReportService>();
        _mockLogger = new Mock<ILogger<ReportsController>>();
        _controller = new ReportsController(_mockService.Object, _mockLogger.Object);
    }

    #region GetTrialBalance Tests

    [Fact]
    public async Task GetTrialBalance_ReturnsOkWithReport()
    {
        // Arrange
        var report = new TrialBalanceReportDto
        {
            Accounts = new()
            {
                new TrialBalanceDto { AccountCode = "1000", AccountName = "Cash", Debit = 1000m, Credit = 0m }
            },
            TotalDebits = 1000m,
            TotalCredits = 1000m,
            IsBalanced = true
        };
        _mockService.Setup(s => s.GetTrialBalanceAsync()).ReturnsAsync(report);

        // Act
        var result = await _controller.GetTrialBalance();

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        var returnedReport = okResult.Value as TrialBalanceReportDto;
        returnedReport!.IsBalanced.Should().BeTrue();
    }

    [Fact]
    public async Task GetTrialBalance_WithImbalancedAccounts_ReturnsImbalancedReport()
    {
        // Arrange
        var report = new TrialBalanceReportDto
        {
            Accounts = new(),
            TotalDebits = 1000m,
            TotalCredits = 500m,
            IsBalanced = false
        };
        _mockService.Setup(s => s.GetTrialBalanceAsync()).ReturnsAsync(report);

        // Act
        var result = await _controller.GetTrialBalance();

        // Assert
        var okResult = result.Result as OkObjectResult;
        var returnedReport = okResult!.Value as TrialBalanceReportDto;
        returnedReport!.IsBalanced.Should().BeFalse();
    }

    #endregion

    #region GetTotalDebits Tests

    [Fact]
    public async Task GetTotalDebits_ReturnsOkWithTotal()
    {
        // Arrange
        _mockService.Setup(s => s.GetTotalDebitAsync()).ReturnsAsync(5000m);

        // Act
        var result = await _controller.GetTotalDebits();

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GetTotalDebits_WithNoEntries_ReturnsZero()
    {
        // Arrange
        _mockService.Setup(s => s.GetTotalDebitAsync()).ReturnsAsync(0m);

        // Act
        var result = await _controller.GetTotalDebits();

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
    }

    #endregion

    #region GetTotalCredits Tests

    [Fact]
    public async Task GetTotalCredits_ReturnsOkWithTotal()
    {
        // Arrange
        _mockService.Setup(s => s.GetTotalCreditAsync()).ReturnsAsync(5000m);

        // Act
        var result = await _controller.GetTotalCredits();

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
    }

    #endregion
}
