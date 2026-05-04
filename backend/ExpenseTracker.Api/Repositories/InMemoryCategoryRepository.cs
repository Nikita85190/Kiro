using System.Collections.Concurrent;
using ExpenseTracker.Api.Models;

namespace ExpenseTracker.Api.Repositories;

public class InMemoryCategoryRepository : ICategoryRepository
{
    private readonly ConcurrentDictionary<Guid, Category> _store = new();

    public Category? GetById(Guid id) => _store.TryGetValue(id, out var c) ? c : null;
    public Category? GetByName(string name) => _store.Values.FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    public IReadOnlyList<Category> GetAll() => _store.Values.ToList().AsReadOnly();
    public void Add(Category category) => _store[category.Id] = category;
    public void Update(Category category) => _store[category.Id] = category;
    public void Delete(Guid id) => _store.TryRemove(id, out _);
}
