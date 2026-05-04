using ExpenseTracker.Api.DTOs;
using ExpenseTracker.Api.Exceptions;
using ExpenseTracker.Api.Models;
using ExpenseTracker.Api.Repositories;

namespace ExpenseTracker.Api.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ITransactionRepository _transactionRepository;

    public CategoryService(
        ICategoryRepository categoryRepository,
        ITransactionRepository transactionRepository)
    {
        _categoryRepository = categoryRepository;
        _transactionRepository = transactionRepository;
    }

    public CategoryResponse Create(CreateCategoryRequest request)
    {
        var existing = _categoryRepository.GetByName(request.Name);
        if (existing != null)
            throw new ConflictException($"Category with name '{request.Name}' already exists.");

        var category = new Category { Name = request.Name };
        _categoryRepository.Add(category);

        return new CategoryResponse(category.Id, category.Name);
    }

    public CategoryResponse Rename(Guid id, RenameCategoryRequest request)
    {
        var category = _categoryRepository.GetById(id)
            ?? throw new NotFoundException($"Category with id '{id}' was not found.");

        var existing = _categoryRepository.GetByName(request.Name);
        if (existing != null && existing.Id != id)
            throw new ConflictException($"Category with name '{request.Name}' already exists.");

        category.Name = request.Name;
        _categoryRepository.Update(category);

        return new CategoryResponse(category.Id, category.Name);
    }

    public void Delete(Guid id)
    {
        var category = _categoryRepository.GetById(id)
            ?? throw new NotFoundException($"Category with id '{id}' was not found.");

        // Check via transaction repository — category repo should not know about transactions (ISP)
        var hasTransactions = _transactionRepository.GetAll().Any(t => t.CategoryId == id);
        if (hasTransactions)
            throw new BusinessRuleException($"Category '{category.Name}' cannot be deleted because it has associated transactions.");

        _categoryRepository.Delete(id);
    }

    public IReadOnlyList<CategoryResponse> GetAll()
    {
        return _categoryRepository.GetAll()
            .Select(c => new CategoryResponse(c.Id, c.Name))
            .ToList();
    }
}
