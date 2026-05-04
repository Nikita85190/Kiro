using ExpenseTracker.Api.Models;

namespace ExpenseTracker.Api.Repositories;

public interface ITransactionRepository
{
    Transaction? GetById(Guid id);
    IReadOnlyList<Transaction> GetAll();
    void Add(Transaction transaction);
    void Update(Transaction transaction);
    void Delete(Guid id);
}
