using System.Collections.Concurrent;
using ExpenseTracker.Api.Models;

namespace ExpenseTracker.Api.Repositories;

public class InMemoryTransactionRepository : ITransactionRepository
{
    private readonly ConcurrentDictionary<Guid, Transaction> _store = new();

    public Transaction? GetById(Guid id) => _store.TryGetValue(id, out var t) ? t : null;
    public IReadOnlyList<Transaction> GetAll() => _store.Values.ToList().AsReadOnly();
    public void Add(Transaction transaction) => _store[transaction.Id] = transaction;
    public void Update(Transaction transaction) => _store[transaction.Id] = transaction;
    public void Delete(Guid id) => _store.TryRemove(id, out _);
}
