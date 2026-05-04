using ExpenseTracker.Api.DTOs;

namespace ExpenseTracker.Api.Services;

public interface ICategoryService
{
    CategoryResponse Create(CreateCategoryRequest request);
    CategoryResponse Rename(Guid id, RenameCategoryRequest request);
    void Delete(Guid id);
    IReadOnlyList<CategoryResponse> GetAll();
}
