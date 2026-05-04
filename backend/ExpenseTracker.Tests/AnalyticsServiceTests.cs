using ExpenseTracker.Api.DTOs;
using ExpenseTracker.Api.Models;
using ExpenseTracker.Api.Repositories;
using ExpenseTracker.Api.Services;
using FluentAssertions;
using Moq;

namespace ExpenseTracker.Tests;

public class AnalyticsServiceTests
{
    private readonly Mock<ITransactionRepository> _transactionRepoMock;
    private readonly Mock<ICategoryRepository> _categoryRepoMock;
    private readonly AnalyticsService _service;

    public AnalyticsServiceTests()
    {
        _transactionRepoMock = new Mock<ITransactionRepository>();
        _categoryRepoMock = new Mock<ICategoryRepository>();
        _service = new AnalyticsService(_transactionRepoMock.Object, _categoryRepoMock.Object);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────

    private static Category MakeCategory(string name = "Food") =>
        new() { Id = Guid.NewGuid(), Name = name };

    private static Transaction MakeTransaction(
        Guid categoryId,
        TransactionType type,
        decimal amount,
        DateOnly? date = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            Type = type,
            Amount = amount,
            Date = date ?? new DateOnly(2024, 6, 1),
            CategoryId = categoryId
        };

    // ─────────────────────────────────────────────────────────────────────────
    // GetBalance
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void GetBalance_NoFilter_ReturnsSumOfAllTransactions()
    {
        // Arrange
        var cat = MakeCategory();
        _transactionRepoMock.Setup(r => r.GetAll()).Returns(new List<Transaction>
        {
            MakeTransaction(cat.Id, TransactionType.Income,  1000m),
            MakeTransaction(cat.Id, TransactionType.Income,   500m),
            MakeTransaction(cat.Id, TransactionType.Expense,  300m),
        });

        // Act
        var result = _service.GetBalance(null, null);

        // Assert
        result.TotalIncome.Should().Be(1500m);
        result.TotalExpenses.Should().Be(300m);
        result.Balance.Should().Be(1200m);
    }

    [Fact]
    public void GetBalance_EmptyRepository_ReturnsZeros()
    {
        // Arrange
        _transactionRepoMock.Setup(r => r.GetAll()).Returns(new List<Transaction>());

        // Act
        var result = _service.GetBalance(null, null);

        // Assert
        result.TotalIncome.Should().Be(0m);
        result.TotalExpenses.Should().Be(0m);
        result.Balance.Should().Be(0m);
    }

    [Fact]
    public void GetBalance_FilterByDateFrom_ExcludesEarlierTransactions()
    {
        // Arrange
        var cat = MakeCategory();
        _transactionRepoMock.Setup(r => r.GetAll()).Returns(new List<Transaction>
        {
            MakeTransaction(cat.Id, TransactionType.Income, 200m, new DateOnly(2024, 1, 1)),
            MakeTransaction(cat.Id, TransactionType.Income, 800m, new DateOnly(2024, 6, 1)),
        });

        // Act
        var result = _service.GetBalance(new DateOnly(2024, 3, 1), null);

        // Assert
        result.TotalIncome.Should().Be(800m);
    }

    [Fact]
    public void GetBalance_FilterByDateTo_ExcludesLaterTransactions()
    {
        // Arrange
        var cat = MakeCategory();
        _transactionRepoMock.Setup(r => r.GetAll()).Returns(new List<Transaction>
        {
            MakeTransaction(cat.Id, TransactionType.Income, 200m, new DateOnly(2024, 1, 1)),
            MakeTransaction(cat.Id, TransactionType.Income, 800m, new DateOnly(2024, 6, 1)),
        });

        // Act
        var result = _service.GetBalance(null, new DateOnly(2024, 3, 1));

        // Assert
        result.TotalIncome.Should().Be(200m);
    }

    [Fact]
    public void GetBalance_BalanceIsIncomeMinus_Expenses()
    {
        // Arrange
        var cat = MakeCategory();
        _transactionRepoMock.Setup(r => r.GetAll()).Returns(new List<Transaction>
        {
            MakeTransaction(cat.Id, TransactionType.Income,  500m),
            MakeTransaction(cat.Id, TransactionType.Expense, 200m),
        });

        // Act
        var result = _service.GetBalance(null, null);

        // Assert
        result.Balance.Should().Be(result.TotalIncome - result.TotalExpenses);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GetReport
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void GetReport_GroupsExpensesByCategory()
    {
        // Arrange
        var food = MakeCategory("Food");
        var transport = MakeCategory("Transport");

        _transactionRepoMock.Setup(r => r.GetAll()).Returns(new List<Transaction>
        {
            MakeTransaction(food.Id,      TransactionType.Expense, 100m),
            MakeTransaction(food.Id,      TransactionType.Expense,  50m),
            MakeTransaction(transport.Id, TransactionType.Expense,  80m),
        });
        _categoryRepoMock.Setup(r => r.GetAll()).Returns(new List<Category> { food, transport });

        // Act
        var result = _service.GetReport(new DateOnly(2024, 1, 1), new DateOnly(2024, 12, 31));

        // Assert
        result.ExpensesByCategory.Should().HaveCount(2);
        result.ExpensesByCategory.Should().ContainSingle(b => b.CategoryName == "Food" && b.Total == 150m);
        result.ExpensesByCategory.Should().ContainSingle(b => b.CategoryName == "Transport" && b.Total == 80m);
    }

    [Fact]
    public void GetReport_GroupsIncomeByCategory()
    {
        // Arrange
        var salary = MakeCategory("Salary");

        _transactionRepoMock.Setup(r => r.GetAll()).Returns(new List<Transaction>
        {
            MakeTransaction(salary.Id, TransactionType.Income, 3000m),
            MakeTransaction(salary.Id, TransactionType.Income, 1000m),
        });
        _categoryRepoMock.Setup(r => r.GetAll()).Returns(new List<Category> { salary });

        // Act
        var result = _service.GetReport(new DateOnly(2024, 1, 1), new DateOnly(2024, 12, 31));

        // Assert
        result.IncomeByCategory.Should().ContainSingle(b => b.CategoryName == "Salary" && b.Total == 4000m);
    }

    [Fact]
    public void GetReport_TotalsMatchSumOfTransactions()
    {
        // Arrange
        var cat = MakeCategory();
        _transactionRepoMock.Setup(r => r.GetAll()).Returns(new List<Transaction>
        {
            MakeTransaction(cat.Id, TransactionType.Income,  2000m),
            MakeTransaction(cat.Id, TransactionType.Expense,  600m),
        });
        _categoryRepoMock.Setup(r => r.GetAll()).Returns(new List<Category> { cat });

        // Act
        var result = _service.GetReport(new DateOnly(2024, 1, 1), new DateOnly(2024, 12, 31));

        // Assert
        result.TotalIncome.Should().Be(2000m);
        result.TotalExpenses.Should().Be(600m);
        result.Balance.Should().Be(1400m);
    }

    [Fact]
    public void GetReport_FilterByDateRange_ExcludesOutOfRangeTransactions()
    {
        // Arrange
        var cat = MakeCategory();
        _transactionRepoMock.Setup(r => r.GetAll()).Returns(new List<Transaction>
        {
            MakeTransaction(cat.Id, TransactionType.Expense, 100m, new DateOnly(2024, 1, 1)),
            MakeTransaction(cat.Id, TransactionType.Expense, 200m, new DateOnly(2024, 6, 1)),
            MakeTransaction(cat.Id, TransactionType.Expense, 300m, new DateOnly(2024, 12, 1)),
        });
        _categoryRepoMock.Setup(r => r.GetAll()).Returns(new List<Category> { cat });

        // Act
        var result = _service.GetReport(new DateOnly(2024, 3, 1), new DateOnly(2024, 9, 1));

        // Assert
        result.TotalExpenses.Should().Be(200m);
    }

    [Fact]
    public void GetReport_EmptyRepository_ReturnsZerosAndEmptyBreakdowns()
    {
        // Arrange
        _transactionRepoMock.Setup(r => r.GetAll()).Returns(new List<Transaction>());
        _categoryRepoMock.Setup(r => r.GetAll()).Returns(new List<Category>());

        // Act
        var result = _service.GetReport(new DateOnly(2024, 1, 1), new DateOnly(2024, 12, 31));

        // Assert
        result.TotalIncome.Should().Be(0m);
        result.TotalExpenses.Should().Be(0m);
        result.Balance.Should().Be(0m);
        result.ExpensesByCategory.Should().BeEmpty();
        result.IncomeByCategory.Should().BeEmpty();
    }
}
