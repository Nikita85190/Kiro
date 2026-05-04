using ExpenseTracker.Api.Models;

namespace ExpenseTracker.Api.Repositories;

public interface ICategoryRepository
{
    Category? GetById(Guid id);
    Category? GetByName(string name);
    IReadOnlyList<Category> GetAll();
    void Add(Category category);
    void Update(Category category);
    void Delete(Guid id);
}
