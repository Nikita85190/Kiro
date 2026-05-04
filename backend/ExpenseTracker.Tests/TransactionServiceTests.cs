using ExpenseTracker.Api.DTOs;
using ExpenseTracker.Api.Exceptions;
using ExpenseTracker.Api.Models;
using ExpenseTracker.Api.Repositories;
using ExpenseTracker.Api.Services;
using FluentAssertions;
using Moq;

namespace ExpenseTracker.Tests;

public class TransactionServiceTests
{
    private readonly Mock<ITransactionRepository> _transactionRepoMock;
    private readonly Mock<ICategoryRepository> _categoryRepoMock;
    private readonly TransactionService _service;

    public TransactionServiceTests()
    {
        _transactionRepoMock = new Mock<ITransactionRepository>();
        _categoryRepoMock = new Mock<ICategoryRepository>();
        _service = new TransactionService(_transactionRepoMock.Object, _categoryRepoMock.Object);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────

    private static Category MakeCategory(string name = "Food") =>
        new() { Id = Guid.NewGuid(), Name = name };

    private static Transaction MakeTransaction(
        Guid? categoryId = null,
        TransactionType type = TransactionType.Expense,
        decimal amount = 100m,
        DateOnly? date = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            Type = type,
            Amount = amount,
            Date = date ?? DateOnly.FromDateTime(DateTime.Today),
            CategoryId = categoryId ?? Guid.NewGuid(),
            Description = null
        };

    // ─────────────────────────────────────────────────────────────────────────
    // Create
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Create_ValidRequest_ReturnsTransactionResponse()
    {
        // Arrange
        var category = MakeCategory("Salary");
        var request = new CreateTransactionRequest(
            Type: "income",
            Amount: 5000m,
            Date: new DateOnly(2024, 6, 1),
            CategoryId: category.Id,
            Description: "June salary");

        _categoryRepoMock.Setup(r => r.GetById(category.Id)).Returns(category);

        // Act
        var result = _service.Create(request);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be("income");
        result.Amount.Should().Be(5000m);
        result.Date.Should().Be(new DateOnly(2024, 6, 1));
        result.CategoryId.Should().Be(category.Id);
        result.CategoryName.Should().Be("Salary");
        result.Description.Should().Be("June salary");
        result.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_ValidRequest_CallsRepositoryAdd()
    {
        // Arrange
        var category = MakeCategory();
        var request = new CreateTransactionRequest(
            Type: "expense",
            Amount: 200m,
            Date: new DateOnly(2024, 5, 15),
            CategoryId: category.Id,
            Description: null);

        _categoryRepoMock.Setup(r => r.GetById(category.Id)).Returns(category);

        // Act
        _service.Create(request);

        // Assert
        _transactionRepoMock.Verify(r => r.Add(It.Is<Transaction>(t =>
            t.Amount == 200m &&
            t.Type == TransactionType.Expense &&
            t.CategoryId == category.Id)), Times.Once);
    }

    [Fact]
    public void Create_CategoryNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var missingCategoryId = Guid.NewGuid();
        var request = new CreateTransactionRequest(
            Type: "expense",
            Amount: 100m,
            Date: new DateOnly(2024, 1, 1),
            CategoryId: missingCategoryId,
            Description: null);

        _categoryRepoMock.Setup(r => r.GetById(missingCategoryId)).Returns((Category?)null);

        // Act
        var act = () => _service.Create(request);

        // Assert
        act.Should().Throw<NotFoundException>()
            .WithMessage($"*{missingCategoryId}*");
    }

    [Fact]
    public void Create_CategoryNotFound_DoesNotCallRepositoryAdd()
    {
        // Arrange
        var missingCategoryId = Guid.NewGuid();
        var request = new CreateTransactionRequest(
            Type: "expense",
            Amount: 100m,
            Date: new DateOnly(2024, 1, 1),
            CategoryId: missingCategoryId,
            Description: null);

        _categoryRepoMock.Setup(r => r.GetById(missingCategoryId)).Returns((Category?)null);

        // Act
        var act = () => _service.Create(request);

        // Assert
        act.Should().Throw<NotFoundException>();
        _transactionRepoMock.Verify(r => r.Add(It.IsAny<Transaction>()), Times.Never);
    }

    [Fact]
    public void Create_TypeIsCaseInsensitive_ParsesCorrectly()
    {
        // Arrange
        var category = MakeCategory();
        var request = new CreateTransactionRequest(
            Type: "INCOME",
            Amount: 100m,
            Date: new DateOnly(2024, 1, 1),
            CategoryId: category.Id,
            Description: null);

        _categoryRepoMock.Setup(r => r.GetById(category.Id)).Returns(category);

        // Act
        var result = _service.Create(request);

        // Assert
        result.Type.Should().Be("income");
    }

    [Fact]
    public void Create_WithNullDescription_StoresNullDescription()
    {
        // Arrange
        var category = MakeCategory();
        var request = new CreateTransactionRequest(
            Type: "expense",
            Amount: 50m,
            Date: new DateOnly(2024, 3, 10),
            CategoryId: category.Id,
            Description: null);

        _categoryRepoMock.Setup(r => r.GetById(category.Id)).Returns(category);

        // Act
        var result = _service.Create(request);

        // Assert
        result.Description.Should().BeNull();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Update
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Update_ValidRequest_ReturnsUpdatedResponse()
    {
        // Arrange
        var category = MakeCategory("Transport");
        var transaction = MakeTransaction(categoryId: category.Id);
        var request = new UpdateTransactionRequest(
            Type: "expense",
            Amount: 350m,
            Date: new DateOnly(2024, 7, 20),
            CategoryId: category.Id,
            Description: "Bus pass");

        _transactionRepoMock.Setup(r => r.GetById(transaction.Id)).Returns(transaction);
        _categoryRepoMock.Setup(r => r.GetById(category.Id)).Returns(category);

        // Act
        var result = _service.Update(transaction.Id, request);

        // Assert
        result.Id.Should().Be(transaction.Id);
        result.Amount.Should().Be(350m);
        result.Date.Should().Be(new DateOnly(2024, 7, 20));
        result.CategoryName.Should().Be("Transport");
        result.Description.Should().Be("Bus pass");
    }

    [Fact]
    public void Update_PreservesOriginalId()
    {
        // Arrange
        var category = MakeCategory();
        var transaction = MakeTransaction(categoryId: category.Id);
        var originalId = transaction.Id;
        var request = new UpdateTransactionRequest(
            Type: "income",
            Amount: 999m,
            Date: new DateOnly(2024, 8, 1),
            CategoryId: category.Id,
            Description: null);

        _transactionRepoMock.Setup(r => r.GetById(originalId)).Returns(transaction);
        _categoryRepoMock.Setup(r => r.GetById(category.Id)).Returns(category);

        // Act
        var result = _service.Update(originalId, request);

        // Assert
        result.Id.Should().Be(originalId);
    }

    [Fact]
    public void Update_TransactionNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var missingId = Guid.NewGuid();
        var request = new UpdateTransactionRequest(
            Type: "expense",
            Amount: 100m,
            Date: new DateOnly(2024, 1, 1),
            CategoryId: Guid.NewGuid(),
            Description: null);

        _transactionRepoMock.Setup(r => r.GetById(missingId)).Returns((Transaction?)null);

        // Act
        var act = () => _service.Update(missingId, request);

        // Assert
        act.Should().Throw<NotFoundException>()
            .WithMessage($"*{missingId}*");
    }

    [Fact]
    public void Update_CategoryNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var transaction = MakeTransaction();
        var missingCategoryId = Guid.NewGuid();
        var request = new UpdateTransactionRequest(
            Type: "expense",
            Amount: 100m,
            Date: new DateOnly(2024, 1, 1),
            CategoryId: missingCategoryId,
            Description: null);

        _transactionRepoMock.Setup(r => r.GetById(transaction.Id)).Returns(transaction);
        _categoryRepoMock.Setup(r => r.GetById(missingCategoryId)).Returns((Category?)null);

        // Act
        var act = () => _service.Update(transaction.Id, request);

        // Assert
        act.Should().Throw<NotFoundException>()
            .WithMessage($"*{missingCategoryId}*");
    }

    [Fact]
    public void Update_ValidRequest_CallsRepositoryUpdate()
    {
        // Arrange
        var category = MakeCategory();
        var transaction = MakeTransaction(categoryId: category.Id);
        var request = new UpdateTransactionRequest(
            Type: "income",
            Amount: 1500m,
            Date: new DateOnly(2024, 9, 5),
            CategoryId: category.Id,
            Description: "Bonus");

        _transactionRepoMock.Setup(r => r.GetById(transaction.Id)).Returns(transaction);
        _categoryRepoMock.Setup(r => r.GetById(category.Id)).Returns(category);

        // Act
        _service.Update(transaction.Id, request);

        // Assert
        _transactionRepoMock.Verify(r => r.Update(It.Is<Transaction>(t =>
            t.Id == transaction.Id &&
            t.Amount == 1500m &&
            t.Type == TransactionType.Income)), Times.Once);
    }

    [Fact]
    public void Update_UpdatesAllFields()
    {
        // Arrange
        var oldCategory = MakeCategory("OldCat");
        var newCategory = MakeCategory("NewCat");
        var transaction = MakeTransaction(categoryId: oldCategory.Id, type: TransactionType.Expense, amount: 50m);

        var request = new UpdateTransactionRequest(
            Type: "income",
            Amount: 999m,
            Date: new DateOnly(2025, 1, 1),
            CategoryId: newCategory.Id,
            Description: "Updated desc");

        _transactionRepoMock.Setup(r => r.GetById(transaction.Id)).Returns(transaction);
        _categoryRepoMock.Setup(r => r.GetById(newCategory.Id)).Returns(newCategory);

        // Act
        var result = _service.Update(transaction.Id, request);

        // Assert
        result.Type.Should().Be("income");
        result.Amount.Should().Be(999m);
        result.Date.Should().Be(new DateOnly(2025, 1, 1));
        result.CategoryId.Should().Be(newCategory.Id);
        result.CategoryName.Should().Be("NewCat");
        result.Description.Should().Be("Updated desc");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Delete
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Delete_ExistingTransaction_CallsRepositoryDelete()
    {
        // Arrange
        var transaction = MakeTransaction();
        _transactionRepoMock.Setup(r => r.GetById(transaction.Id)).Returns(transaction);

        // Act
        _service.Delete(transaction.Id);

        // Assert
        _transactionRepoMock.Verify(r => r.Delete(transaction.Id), Times.Once);
    }

    [Fact]
    public void Delete_TransactionNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var missingId = Guid.NewGuid();
        _transactionRepoMock.Setup(r => r.GetById(missingId)).Returns((Transaction?)null);

        // Act
        var act = () => _service.Delete(missingId);

        // Assert
        act.Should().Throw<NotFoundException>()
            .WithMessage($"*{missingId}*");
    }

    [Fact]
    public void Delete_TransactionNotFound_DoesNotCallRepositoryDelete()
    {
        // Arrange
        var missingId = Guid.NewGuid();
        _transactionRepoMock.Setup(r => r.GetById(missingId)).Returns((Transaction?)null);

        // Act
        var act = () => _service.Delete(missingId);

        // Assert
        act.Should().Throw<NotFoundException>();
        _transactionRepoMock.Verify(r => r.Delete(It.IsAny<Guid>()), Times.Never);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GetAll — sorting
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void GetAll_NoFilter_ReturnsSortedByDateDescending()
    {
        // Arrange
        var category = MakeCategory();
        var t1 = MakeTransaction(categoryId: category.Id, date: new DateOnly(2024, 1, 1));
        var t2 = MakeTransaction(categoryId: category.Id, date: new DateOnly(2024, 3, 15));
        var t3 = MakeTransaction(categoryId: category.Id, date: new DateOnly(2024, 2, 10));

        _transactionRepoMock.Setup(r => r.GetAll()).Returns(new List<Transaction> { t1, t2, t3 });
        _categoryRepoMock.Setup(r => r.GetById(category.Id)).Returns(category);

        // Act
        var result = _service.GetAll(new TransactionFilter());

        // Assert
        result.Items.Should().HaveCount(3);
        result.Items[0].Date.Should().Be(new DateOnly(2024, 3, 15));
        result.Items[1].Date.Should().Be(new DateOnly(2024, 2, 10));
        result.Items[2].Date.Should().Be(new DateOnly(2024, 1, 1));
    }

    [Fact]
    public void GetAll_NoFilter_ReturnsTotalCount()
    {
        // Arrange
        var category = MakeCategory();
        var transactions = Enumerable.Range(1, 5)
            .Select(i => MakeTransaction(categoryId: category.Id, date: new DateOnly(2024, i, 1)))
            .ToList();

        _transactionRepoMock.Setup(r => r.GetAll()).Returns(transactions);
        _categoryRepoMock.Setup(r => r.GetById(category.Id)).Returns(category);

        // Act
        var result = _service.GetAll(new TransactionFilter());

        // Assert
        result.TotalCount.Should().Be(5);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GetAll — date filters
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void GetAll_FilterByDateFrom_ReturnsOnlyTransactionsOnOrAfterDate()
    {
        // Arrange
        var category = MakeCategory();
        var t1 = MakeTransaction(categoryId: category.Id, date: new DateOnly(2024, 1, 1));
        var t2 = MakeTransaction(categoryId: category.Id, date: new DateOnly(2024, 3, 1));
        var t3 = MakeTransaction(categoryId: category.Id, date: new DateOnly(2024, 5, 1));

        _transactionRepoMock.Setup(r => r.GetAll()).Returns(new List<Transaction> { t1, t2, t3 });
        _categoryRepoMock.Setup(r => r.GetById(category.Id)).Returns(category);

        var filter = new TransactionFilter(DateFrom: new DateOnly(2024, 3, 1));

        // Act
        var result = _service.GetAll(filter);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.Should().AllSatisfy(t => (t.Date >= new DateOnly(2024, 3, 1)).Should().BeTrue());
    }

    [Fact]
    public void GetAll_FilterByDateTo_ReturnsOnlyTransactionsOnOrBeforeDate()
    {
        // Arrange
        var category = MakeCategory();
        var t1 = MakeTransaction(categoryId: category.Id, date: new DateOnly(2024, 1, 1));
        var t2 = MakeTransaction(categoryId: category.Id, date: new DateOnly(2024, 3, 1));
        var t3 = MakeTransaction(categoryId: category.Id, date: new DateOnly(2024, 5, 1));

        _transactionRepoMock.Setup(r => r.GetAll()).Returns(new List<Transaction> { t1, t2, t3 });
        _categoryRepoMock.Setup(r => r.GetById(category.Id)).Returns(category);

        var filter = new TransactionFilter(DateTo: new DateOnly(2024, 3, 1));

        // Act
        var result = _service.GetAll(filter);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.Should().AllSatisfy(t => (t.Date <= new DateOnly(2024, 3, 1)).Should().BeTrue());
    }

    [Fact]
    public void GetAll_FilterByDateRange_ReturnsOnlyTransactionsInRange()
    {
        // Arrange
        var category = MakeCategory();
        var t1 = MakeTransaction(categoryId: category.Id, date: new DateOnly(2024, 1, 1));
        var t2 = MakeTransaction(categoryId: category.Id, date: new DateOnly(2024, 3, 15));
        var t3 = MakeTransaction(categoryId: category.Id, date: new DateOnly(2024, 6, 30));

        _transactionRepoMock.Setup(r => r.GetAll()).Returns(new List<Transaction> { t1, t2, t3 });
        _categoryRepoMock.Setup(r => r.GetById(category.Id)).Returns(category);

        var filter = new TransactionFilter(
            DateFrom: new DateOnly(2024, 2, 1),
            DateTo: new DateOnly(2024, 5, 31));

        // Act
        var result = _service.GetAll(filter);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].Date.Should().Be(new DateOnly(2024, 3, 15));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GetAll — category filter
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void GetAll_FilterByCategoryId_ReturnsOnlyMatchingTransactions()
    {
        // Arrange
        var cat1 = MakeCategory("Food");
        var cat2 = MakeCategory("Transport");
        var t1 = MakeTransaction(categoryId: cat1.Id);
        var t2 = MakeTransaction(categoryId: cat2.Id);
        var t3 = MakeTransaction(categoryId: cat1.Id);

        _transactionRepoMock.Setup(r => r.GetAll()).Returns(new List<Transaction> { t1, t2, t3 });
        _categoryRepoMock.Setup(r => r.GetById(cat1.Id)).Returns(cat1);
        _categoryRepoMock.Setup(r => r.GetById(cat2.Id)).Returns(cat2);

        var filter = new TransactionFilter(CategoryId: cat1.Id);

        // Act
        var result = _service.GetAll(filter);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.Should().AllSatisfy(t => t.CategoryId.Should().Be(cat1.Id));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GetAll — type filter
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void GetAll_FilterByTypeIncome_ReturnsOnlyIncomeTransactions()
    {
        // Arrange
        var category = MakeCategory();
        var t1 = MakeTransaction(categoryId: category.Id, type: TransactionType.Income);
        var t2 = MakeTransaction(categoryId: category.Id, type: TransactionType.Expense);
        var t3 = MakeTransaction(categoryId: category.Id, type: TransactionType.Income);

        _transactionRepoMock.Setup(r => r.GetAll()).Returns(new List<Transaction> { t1, t2, t3 });
        _categoryRepoMock.Setup(r => r.GetById(category.Id)).Returns(category);

        var filter = new TransactionFilter(Type: TransactionType.Income);

        // Act
        var result = _service.GetAll(filter);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.Should().AllSatisfy(t => t.Type.Should().Be("income"));
    }

    [Fact]
    public void GetAll_FilterByTypeExpense_ReturnsOnlyExpenseTransactions()
    {
        // Arrange
        var category = MakeCategory();
        var t1 = MakeTransaction(categoryId: category.Id, type: TransactionType.Income);
        var t2 = MakeTransaction(categoryId: category.Id, type: TransactionType.Expense);
        var t3 = MakeTransaction(categoryId: category.Id, type: TransactionType.Expense);

        _transactionRepoMock.Setup(r => r.GetAll()).Returns(new List<Transaction> { t1, t2, t3 });
        _categoryRepoMock.Setup(r => r.GetById(category.Id)).Returns(category);

        var filter = new TransactionFilter(Type: TransactionType.Expense);

        // Act
        var result = _service.GetAll(filter);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.Should().AllSatisfy(t => t.Type.Should().Be("expense"));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GetAll — pagination
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void GetAll_Pagination_ReturnsCorrectPage()
    {
        // Arrange
        var category = MakeCategory();
        var transactions = Enumerable.Range(1, 10)
            .Select(i => MakeTransaction(categoryId: category.Id, date: new DateOnly(2024, 1, i)))
            .ToList();

        _transactionRepoMock.Setup(r => r.GetAll()).Returns(transactions);
        _categoryRepoMock.Setup(r => r.GetById(category.Id)).Returns(category);

        var filter = new TransactionFilter(Page: 2, PageSize: 3);

        // Act
        var result = _service.GetAll(filter);

        // Assert
        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(10);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(3);
    }

    [Fact]
    public void GetAll_Pagination_LastPageMayHaveFewerItems()
    {
        // Arrange
        var category = MakeCategory();
        var transactions = Enumerable.Range(1, 7)
            .Select(i => MakeTransaction(categoryId: category.Id, date: new DateOnly(2024, 1, i)))
            .ToList();

        _transactionRepoMock.Setup(r => r.GetAll()).Returns(transactions);
        _categoryRepoMock.Setup(r => r.GetById(category.Id)).Returns(category);

        var filter = new TransactionFilter(Page: 3, PageSize: 3);

        // Act
        var result = _service.GetAll(filter);

        // Assert
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(7);
    }

    [Fact]
    public void GetAll_Pagination_TotalCountReflectsFilteredCount()
    {
        // Arrange
        var category = MakeCategory();
        var t1 = MakeTransaction(categoryId: category.Id, type: TransactionType.Income);
        var t2 = MakeTransaction(categoryId: category.Id, type: TransactionType.Expense);
        var t3 = MakeTransaction(categoryId: category.Id, type: TransactionType.Income);
        var t4 = MakeTransaction(categoryId: category.Id, type: TransactionType.Income);

        _transactionRepoMock.Setup(r => r.GetAll()).Returns(new List<Transaction> { t1, t2, t3, t4 });
        _categoryRepoMock.Setup(r => r.GetById(category.Id)).Returns(category);

        var filter = new TransactionFilter(Type: TransactionType.Income, Page: 1, PageSize: 2);

        // Act
        var result = _service.GetAll(filter);

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(3); // 3 income transactions total
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GetAll — combined filters
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void GetAll_CombinedFilters_AppliesAllFilters()
    {
        // Arrange
        var cat1 = MakeCategory("Food");
        var cat2 = MakeCategory("Transport");

        var t1 = MakeTransaction(categoryId: cat1.Id, type: TransactionType.Expense, date: new DateOnly(2024, 3, 1));
        var t2 = MakeTransaction(categoryId: cat1.Id, type: TransactionType.Income, date: new DateOnly(2024, 3, 5));
        var t3 = MakeTransaction(categoryId: cat2.Id, type: TransactionType.Expense, date: new DateOnly(2024, 3, 10));
        var t4 = MakeTransaction(categoryId: cat1.Id, type: TransactionType.Expense, date: new DateOnly(2024, 1, 1));

        _transactionRepoMock.Setup(r => r.GetAll()).Returns(new List<Transaction> { t1, t2, t3, t4 });
        _categoryRepoMock.Setup(r => r.GetById(cat1.Id)).Returns(cat1);
        _categoryRepoMock.Setup(r => r.GetById(cat2.Id)).Returns(cat2);

        var filter = new TransactionFilter(
            DateFrom: new DateOnly(2024, 2, 1),
            DateTo: new DateOnly(2024, 4, 30),
            CategoryId: cat1.Id,
            Type: TransactionType.Expense);

        // Act
        var result = _service.GetAll(filter);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].CategoryId.Should().Be(cat1.Id);
        result.Items[0].Type.Should().Be("expense");
    }

    [Fact]
    public void GetAll_EmptyRepository_ReturnsEmptyPagedResult()
    {
        // Arrange
        _transactionRepoMock.Setup(r => r.GetAll()).Returns(new List<Transaction>());

        // Act
        var result = _service.GetAll(new TransactionFilter());

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public void GetAll_IncludesCategoryNameInResponse()
    {
        // Arrange
        var category = MakeCategory("Groceries");
        var transaction = MakeTransaction(categoryId: category.Id);

        _transactionRepoMock.Setup(r => r.GetAll()).Returns(new List<Transaction> { transaction });
        _categoryRepoMock.Setup(r => r.GetById(category.Id)).Returns(category);

        // Act
        var result = _service.GetAll(new TransactionFilter());

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].CategoryName.Should().Be("Groceries");
    }
}
