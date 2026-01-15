using System;
using System.Collections.Generic;
using LedgerX.DTOs;
using LedgerX.Models;

namespace LedgerX.Tests.Fixtures;

/// <summary>
/// Test data fixtures for common test scenarios
/// </summary>
public class TestDataFixture
{
    public static CreateAccountDto ValidAccountRequest => new()
    {
        Code = "1000",
        Name = "Test Cash Account",
        Type = "Asset",
        Balance = 10000m
    };

    public static Account SampleAccount => new()
    {
        Id = Guid.NewGuid().ToString(),
        Code = "1000",
        Name = "Cash",
        Type = "Asset",
        Balance = 50000m,
        CreatedAt = DateTime.UtcNow,
        IsActive = true
    };

    public static List<Account> SampleAccounts => new()
    {
        new Account { Id = "1", Code = "1000", Name = "Cash", Type = "Asset", Balance = 50000m },
        new Account { Id = "2", Code = "2000", Name = "Accounts Payable", Type = "Liability", Balance = 15000m },
        new Account { Id = "3", Code = "3000", Name = "Retained Earnings", Type = "Equity", Balance = 100000m }
    };

    public static CreateJournalEntryDto ValidJournalEntryRequest => new()
    {
        Description = "Test Journal Entry",
        ReferenceNumber = "JE-TEST-001",
        EntryDate = DateTime.UtcNow,
        Lines = new()
        {
            new JournalLineDto { AccountCode = "1000", AccountId = "1", DebitAmount = 1000m, Narration = "Debit" },
            new JournalLineDto { AccountCode = "2000", AccountId = "2", CreditAmount = 1000m, Narration = "Credit" }
        }
    };

    public static JournalEntry SampleJournalEntry => new()
    {
        Id = Guid.NewGuid().ToString(),
        Description = "Sales Revenue",
        ReferenceNumber = "JE-001",
        EntryDate = DateTime.UtcNow,
        IsPosted = false,
        Lines = new()
        {
            new JournalLine { AccountCode = "1000", AccountId = "1", DebitAmount = 5000m, Narration = "Debit Cash" },
            new JournalLine { AccountCode = "4000", AccountId = "2", CreditAmount = 5000m, Narration = "Credit Sales" }
        }
    };

    public static List<JournalEntry> SampleJournalEntries => new()
    {
        new JournalEntry
        {
            Id = "1",
            Description = "Entry 1",
            ReferenceNumber = "JE-001",
            EntryDate = new DateTime(2024, 1, 1),
            IsPosted = true,
            Lines = new()
            {
                new JournalLine { AccountCode = "1000", AccountId = "1", DebitAmount = 1000m },
                new JournalLine { AccountCode = "2000", AccountId = "2", CreditAmount = 1000m }
            }
        },
        new JournalEntry
        {
            Id = "2",
            Description = "Entry 2",
            ReferenceNumber = "JE-002",
            EntryDate = new DateTime(2024, 1, 5),
            IsPosted = true,
            Lines = new()
            {
                new JournalLine { AccountCode = "1000", AccountId = "1", DebitAmount = 2000m },
                new JournalLine { AccountCode = "3000", AccountId = "3", CreditAmount = 2000m }
            }
        }
    };

    public static CreateLedgerKeyDto ValidLedgerKeyRequest => new()
    {
        KeyName = "Test Key",
        ExpiresAt = DateTime.UtcNow.AddYears(1)
    };

    public static LedgerKey SampleLedgerKey => new()
    {
        Id = Guid.NewGuid().ToString(),
        KeyName = "Production Key",
        EncryptionKey = Convert.ToBase64String(new byte[32]),
        CreatedAt = DateTime.UtcNow,
        ExpiresAt = DateTime.UtcNow.AddYears(1),
        IsActive = true
    };
}
