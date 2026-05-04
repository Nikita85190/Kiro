namespace ExpenseTracker.Api.Models;

public record TransactionFilter(
    DateOnly? DateFrom = null,
    DateOnly? DateTo = null,
    Guid? CategoryId = null,
    TransactionType? Type = null,
    int Page = 1,
    int PageSize = 20
);
