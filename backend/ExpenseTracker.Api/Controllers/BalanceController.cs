using Microsoft.AspNetCore.Mvc;
using ExpenseTracker.Api.DTOs;
using ExpenseTracker.Api.Services;

namespace ExpenseTracker.Api.Controllers;

[ApiController]
[Route("api/balance")]
public class BalanceController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public BalanceController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [HttpGet]
    public ActionResult<BalanceResponse> GetBalance(
        [FromQuery] DateOnly? dateFrom,
        [FromQuery] DateOnly? dateTo)
    {
        var result = _analyticsService.GetBalance(dateFrom, dateTo);
        return Ok(result);
    }
}
