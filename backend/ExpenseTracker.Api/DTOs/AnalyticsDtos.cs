namespace ExpenseTracker.Api.DTOs;

public record BalanceResponse(decimal TotalIncome, decimal TotalExpenses, decimal Balance);

public record ReportResponse(
    decimal TotalIncome,
    decimal TotalExpenses,
    decimal Balance,
    IReadOnlyList<CategoryBreakdown> ExpensesByCategory,
    IReadOnlyList<CategoryBreakdown> IncomeByCategory
);

public record CategoryBreakdown(Guid CategoryId, string CategoryName, decimal Total);
