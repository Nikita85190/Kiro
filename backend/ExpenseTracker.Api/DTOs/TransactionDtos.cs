using ExpenseTracker.Api.Models;

namespace ExpenseTracker.Api.DTOs;

public record CreateTransactionRequest(
    string Type,
    decimal Amount,
    DateOnly Date,
    Guid CategoryId,
    string? Description
);

public record UpdateTransactionRequest(
    string Type,
    decimal Amount,
    DateOnly Date,
    Guid CategoryId,
    string? Description
);

public record TransactionResponse(
    Guid Id,
    string Type,
    decimal Amount,
    DateOnly Date,
    Guid CategoryId,
    string CategoryName,
    string? Description,
    DateTime CreatedAt
);

public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize
);
