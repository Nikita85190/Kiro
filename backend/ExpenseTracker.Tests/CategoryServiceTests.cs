using ExpenseTracker.Api.DTOs;
using ExpenseTracker.Api.Exceptions;
using ExpenseTracker.Api.Models;
using ExpenseTracker.Api.Repositories;
using ExpenseTracker.Api.Services;
using FluentAssertions;
using Moq;

namespace ExpenseTracker.Tests;

public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _categoryRepoMock;
    private readonly Mock<ITransactionRepository> _transactionRepoMock;
    private readonly CategoryService _service;

    public CategoryServiceTests()
    {
        _categoryRepoMock = new Mock<ICategoryRepository>();
        _transactionRepoMock = new Mock<ITransactionRepository>();
        _service = new CategoryService(_categoryRepoMock.Object, _transactionRepoMock.Object);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────

    private static Category MakeCategory(string name = "Food") =>
        new() { Id = Guid.NewGuid(), Name = name };

    // ─────────────────────────────────────────────────────────────────────────
    // Create
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Create_NewName_ReturnsCategoryResponse()
    {
        // Arrange
        var request = new CreateCategoryRequest("Food");
        _categoryRepoMock.Setup(r => r.GetByName("Food")).Returns((Category?)null);

        // Act
        var result = _service.Create(request);

        // Assert
        result.Name.Should().Be("Food");
        result.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_NewName_CallsRepositoryAdd()
    {
        // Arrange
        var request = new CreateCategoryRequest("Transport");
        _categoryRepoMock.Setup(r => r.GetByName("Transport")).Returns((Category?)null);

        // Act
        _service.Create(request);

        // Assert
        _categoryRepoMock.Verify(r => r.Add(It.Is<Category>(c => c.Name == "Transport")), Times.Once);
    }

    [Fact]
    public void Create_DuplicateName_ThrowsConflictException()
    {
        // Arrange
        var existing = MakeCategory("Food");
        _categoryRepoMock.Setup(r => r.GetByName("Food")).Returns(existing);

        // Act
        var act = () => _service.Create(new CreateCategoryRequest("Food"));

        // Assert
        act.Should().Throw<ConflictException>().WithMessage("*Food*");
    }

    [Fact]
    public void Create_DuplicateName_DoesNotCallRepositoryAdd()
    {
        // Arrange
        var existing = MakeCategory("Food");
        _categoryRepoMock.Setup(r => r.GetByName("Food")).Returns(existing);

        // Act
        var act = () => _service.Create(new CreateCategoryRequest("Food"));

        // Assert
        act.Should().Throw<ConflictException>();
        _categoryRepoMock.Verify(r => r.Add(It.IsAny<Category>()), Times.Never);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Rename
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Rename_ValidRequest_ReturnsUpdatedResponse()
    {
        // Arrange
        var category = MakeCategory("OldName");
        _categoryRepoMock.Setup(r => r.GetById(category.Id)).Returns(category);
        _categoryRepoMock.Setup(r => r.GetByName("NewName")).Returns((Category?)null);

        // Act
        var result = _service.Rename(category.Id, new RenameCategoryRequest("NewName"));

        // Assert
        result.Id.Should().Be(category.Id);
        result.Name.Should().Be("NewName");
    }

    [Fact]
    public void Rename_CategoryNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var missingId = Guid.NewGuid();
        _categoryRepoMock.Setup(r => r.GetById(missingId)).Returns((Category?)null);

        // Act
        var act = () => _service.Rename(missingId, new RenameCategoryRequest("NewName"));

        // Assert
        act.Should().Throw<NotFoundException>().WithMessage($"*{missingId}*");
    }

    [Fact]
    public void Rename_NameTakenByAnotherCategory_ThrowsConflictException()
    {
        // Arrange
        var category = MakeCategory("OldName");
        var other = MakeCategory("TakenName");
        _categoryRepoMock.Setup(r => r.GetById(category.Id)).Returns(category);
        _categoryRepoMock.Setup(r => r.GetByName("TakenName")).Returns(other);

        // Act
        var act = () => _service.Rename(category.Id, new RenameCategoryRequest("TakenName"));

        // Assert
        act.Should().Throw<ConflictException>().WithMessage("*TakenName*");
    }

    [Fact]
    public void Rename_SameNameAsSelf_DoesNotThrow()
    {
        // Renaming to the same name should be allowed (idempotent)
        // Arrange
        var category = MakeCategory("Food");
        _categoryRepoMock.Setup(r => r.GetById(category.Id)).Returns(category);
        _categoryRepoMock.Setup(r => r.GetByName("Food")).Returns(category); // same instance

        // Act
        var act = () => _service.Rename(category.Id, new RenameCategoryRequest("Food"));

        // Assert
        act.Should().NotThrow();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Delete
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Delete_CategoryWithNoTransactions_CallsRepositoryDelete()
    {
        // Arrange
        var category = MakeCategory();
        _categoryRepoMock.Setup(r => r.GetById(category.Id)).Returns(category);
        _transactionRepoMock.Setup(r => r.GetAll()).Returns(new List<Transaction>());

        // Act
        _service.Delete(category.Id);

        // Assert
        _categoryRepoMock.Verify(r => r.Delete(category.Id), Times.Once);
    }

    [Fact]
    public void Delete_CategoryNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var missingId = Guid.NewGuid();
        _categoryRepoMock.Setup(r => r.GetById(missingId)).Returns((Category?)null);

        // Act
        var act = () => _service.Delete(missingId);

        // Assert
        act.Should().Throw<NotFoundException>().WithMessage($"*{missingId}*");
    }

    [Fact]
    public void Delete_CategoryWithTransactions_ThrowsBusinessRuleException()
    {
        // Arrange
        var category = MakeCategory("Food");
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            Type = TransactionType.Expense,
            Amount = 50m,
            Date = DateOnly.FromDateTime(DateTime.Today),
            CategoryId = category.Id
        };

        _categoryRepoMock.Setup(r => r.GetById(category.Id)).Returns(category);
        _transactionRepoMock.Setup(r => r.GetAll()).Returns(new List<Transaction> { transaction });

        // Act
        var act = () => _service.Delete(category.Id);

        // Assert
        act.Should().Throw<BusinessRuleException>().WithMessage("*Food*");
    }

    [Fact]
    public void Delete_CategoryWithTransactions_DoesNotCallRepositoryDelete()
    {
        // Arrange
        var category = MakeCategory();
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            Type = TransactionType.Expense,
            Amount = 10m,
            Date = DateOnly.FromDateTime(DateTime.Today),
            CategoryId = category.Id
        };

        _categoryRepoMock.Setup(r => r.GetById(category.Id)).Returns(category);
        _transactionRepoMock.Setup(r => r.GetAll()).Returns(new List<Transaction> { transaction });

        // Act
        var act = () => _service.Delete(category.Id);

        // Assert
        act.Should().Throw<BusinessRuleException>();
        _categoryRepoMock.Verify(r => r.Delete(It.IsAny<Guid>()), Times.Never);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GetAll
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void GetAll_ReturnsAllCategories()
    {
        // Arrange
        var categories = new List<Category>
        {
            MakeCategory("Food"),
            MakeCategory("Transport"),
            MakeCategory("Utilities")
        };
        _categoryRepoMock.Setup(r => r.GetAll()).Returns(categories);

        // Act
        var result = _service.GetAll();

        // Assert
        result.Should().HaveCount(3);
        result.Select(c => c.Name).Should().BeEquivalentTo("Food", "Transport", "Utilities");
    }

    [Fact]
    public void GetAll_EmptyRepository_ReturnsEmptyList()
    {
        // Arrange
        _categoryRepoMock.Setup(r => r.GetAll()).Returns(new List<Category>());

        // Act
        var result = _service.GetAll();

        // Assert
        result.Should().BeEmpty();
    }
}
