using ExpenseTracker.Api.DTOs;
using ExpenseTracker.Api.Models;
using ExpenseTracker.Api.Repositories;

namespace ExpenseTracker.Api.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICategoryRepository _categoryRepository;

    public AnalyticsService(
        ITransactionRepository transactionRepository,
        ICategoryRepository categoryRepository)
    {
        _transactionRepository = transactionRepository;
        _categoryRepository = categoryRepository;
    }

    public BalanceResponse GetBalance(DateOnly? from, DateOnly? to)
    {
        // Materialise once to avoid multiple enumeration
        var transactions = GetFilteredTransactions(from, to);

        var totalIncome = transactions
            .Where(t => t.Type == TransactionType.Income)
            .Sum(t => t.Amount);

        var totalExpenses = transactions
            .Where(t => t.Type == TransactionType.Expense)
            .Sum(t => t.Amount);

        return new BalanceResponse(totalIncome, totalExpenses, totalIncome - totalExpenses);
    }

    public ReportResponse GetReport(DateOnly from, DateOnly to)
    {
        // Materialise once — avoids multiple enumeration and N+1 category lookups
        var transactions = GetFilteredTransactions(from, to);

        // Build category name lookup in a single pass — avoids N+1 GetById calls
        var categoryNames = _categoryRepository.GetAll()
            .ToDictionary(c => c.Id, c => c.Name);

        var totalIncome = transactions
            .Where(t => t.Type == TransactionType.Income)
            .Sum(t => t.Amount);

        var totalExpenses = transactions
            .Where(t => t.Type == TransactionType.Expense)
            .Sum(t => t.Amount);

        var expensesByCategory = transactions
            .Where(t => t.Type == TransactionType.Expense)
            .GroupBy(t => t.CategoryId)
            .Select(g => new CategoryBreakdown(
                g.Key,
                categoryNames.GetValueOrDefault(g.Key, string.Empty),
                g.Sum(t => t.Amount)))
            .ToList();

        var incomeByCategory = transactions
            .Where(t => t.Type == TransactionType.Income)
            .GroupBy(t => t.CategoryId)
            .Select(g => new CategoryBreakdown(
                g.Key,
                categoryNames.GetValueOrDefault(g.Key, string.Empty),
                g.Sum(t => t.Amount)))
            .ToList();

        return new ReportResponse(
            totalIncome,
            totalExpenses,
            totalIncome - totalExpenses,
            expensesByCategory,
            incomeByCategory
        );
    }

    private List<Transaction> GetFilteredTransactions(DateOnly? from, DateOnly? to)
    {
        IEnumerable<Transaction> query = _transactionRepository.GetAll();

        if (from.HasValue)
            query = query.Where(t => t.Date >= from.Value);

        if (to.HasValue)
            query = query.Where(t => t.Date <= to.Value);

        // Materialise here so callers don't re-enumerate the source
        return query.ToList();
    }
}
