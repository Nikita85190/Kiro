using Microsoft.AspNetCore.Mvc;
using ExpenseTracker.Api.DTOs;
using ExpenseTracker.Api.Models;
using ExpenseTracker.Api.Services;

namespace ExpenseTracker.Api.Controllers;

[ApiController]
[Route("api/transactions")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public TransactionsController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [HttpGet]
    public ActionResult<PagedResult<TransactionResponse>> GetAll(
        [FromQuery] DateOnly? dateFrom,
        [FromQuery] DateOnly? dateTo,
        [FromQuery] Guid? categoryId,
        [FromQuery] string? type,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        TransactionType? transactionType = null;
        if (!string.IsNullOrEmpty(type))
        {
            if (!Enum.TryParse<TransactionType>(type, ignoreCase: true, out var parsed))
                return BadRequest(new { error = "VALIDATION_ERROR", message = $"Invalid type value: '{type}'. Must be 'income' or 'expense'.", details = (object?)null });
            transactionType = parsed;
        }

        var filter = new TransactionFilter(dateFrom, dateTo, categoryId, transactionType, page, pageSize);
        var result = _transactionService.GetAll(filter);
        return Ok(result);
    }

    [HttpPost]
    public ActionResult<TransactionResponse> Create([FromBody] CreateTransactionRequest request)
    {
        var response = _transactionService.Create(request);
        return CreatedAtAction(nameof(GetAll), new { }, response);
    }

    [HttpPut("{id:guid}")]
    public ActionResult<TransactionResponse> Update(Guid id, [FromBody] UpdateTransactionRequest request)
    {
        var response = _transactionService.Update(id, request);
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id)
    {
        _transactionService.Delete(id);
        return NoContent();
    }
}
