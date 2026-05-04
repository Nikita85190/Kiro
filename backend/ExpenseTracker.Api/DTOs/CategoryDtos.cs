namespace ExpenseTracker.Api.DTOs;

public record CreateCategoryRequest(string Name);
public record RenameCategoryRequest(string Name);
public record CategoryResponse(Guid Id, string Name);
