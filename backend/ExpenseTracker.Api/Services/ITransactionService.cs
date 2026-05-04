using ExpenseTracker.Api.DTOs;
using ExpenseTracker.Api.Models;

namespace ExpenseTracker.Api.Services;

public interface ITransactionService
{
    TransactionResponse Create(CreateTransactionRequest request);
    TransactionResponse Update(Guid id, UpdateTransactionRequest request);
    void Delete(Guid id);
    PagedResult<TransactionResponse> GetAll(TransactionFilter filter);
}
