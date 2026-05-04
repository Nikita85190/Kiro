using ExpenseTracker.Api.DTOs;
using ExpenseTracker.Api.Exceptions;
using ExpenseTracker.Api.Models;
using ExpenseTracker.Api.Repositories;

namespace ExpenseTracker.Api.Services;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICategoryRepository _categoryRepository;

    public TransactionService(
        ITransactionRepository transactionRepository,
        ICategoryRepository categoryRepository)
    {
        _transactionRepository = transactionRepository;
        _categoryRepository = categoryRepository;
    }

    public TransactionResponse Create(CreateTransactionRequest request)
    {
        var category = _categoryRepository.GetById(request.CategoryId)
            ?? throw new NotFoundException($"Category with id '{request.CategoryId}' was not found.");

        var transaction = new Transaction
        {
            Type = ParseTransactionType(request.Type),
            Amount = request.Amount,
            Date = request.Date,
            CategoryId = request.CategoryId,
            Description = request.Description
        };

        _transactionRepository.Add(transaction);

        return MapToResponse(transaction, category.Name);
    }

    public TransactionResponse Update(Guid id, UpdateTransactionRequest request)
    {
        var transaction = _transactionRepository.GetById(id)
            ?? throw new NotFoundException($"Transaction with id '{id}' was not found.");

        var category = _categoryRepository.GetById(request.CategoryId)
            ?? throw new NotFoundException($"Category with id '{request.CategoryId}' was not found.");

        transaction.Type = ParseTransactionType(request.Type);
        transaction.Amount = request.Amount;
        transaction.Date = request.Date;
        transaction.CategoryId = request.CategoryId;
        transaction.Description = request.Description;

        _transactionRepository.Update(transaction);

        return MapToResponse(transaction, category.Name);
    }

    public void Delete(Guid id)
    {
        var transaction = _transactionRepository.GetById(id)
            ?? throw new NotFoundException($"Transaction with id '{id}' was not found.");

        _transactionRepository.Delete(transaction.Id);
    }

    public PagedResult<TransactionResponse> GetAll(TransactionFilter filter)
    {
        // Clamp pagination to safe values
        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);

        // Apply filters
        IEnumerable<Transaction> query = _transactionRepository.GetAll();

        if (filter.DateFrom.HasValue)
            query = query.Where(t => t.Date >= filter.DateFrom.Value);

        if (filter.DateTo.HasValue)
            query = query.Where(t => t.Date <= filter.DateTo.Value);

        if (filter.CategoryId.HasValue)
            query = query.Where(t => t.CategoryId == filter.CategoryId.Value);

        if (filter.Type.HasValue)
            query = query.Where(t => t.Type == filter.Type.Value);

        // Sort by date descending, then by creation time for stable ordering
        var filtered = query
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.CreatedAt)
            .ToList();

        var totalCount = filtered.Count;

        // Paginate
        var items = filtered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // Build category name lookup in a single pass — avoids N+1 GetById calls
        var categoryIds = items.Select(t => t.CategoryId).Distinct().ToList();
        var categoryNames = categoryIds
            .Select(cid => _categoryRepository.GetById(cid))
            .Where(c => c != null)
            .ToDictionary(c => c!.Id, c => c!.Name);

        var responses = items
            .Select(t => MapToResponse(t, categoryNames.GetValueOrDefault(t.CategoryId, string.Empty)))
            .ToList();

        return new PagedResult<TransactionResponse>(responses, totalCount, page, pageSize);
    }

    /// <summary>Parses a transaction type string case-insensitively. Throws BusinessRuleException on invalid input.</summary>
    private static TransactionType ParseTransactionType(string type)
    {
        if (!Enum.TryParse<TransactionType>(type, ignoreCase: true, out var result))
            throw new BusinessRuleException($"Invalid transaction type: '{type}'. Must be 'income' or 'expense'.");
        return result;
    }

    private static TransactionResponse MapToResponse(Transaction transaction, string categoryName) =>
        new(
            transaction.Id,
            transaction.Type.ToString().ToLowerInvariant(),
            transaction.Amount,
            transaction.Date,
            transaction.CategoryId,
            categoryName,
            transaction.Description,
            transaction.CreatedAt
        );
}
