using Microsoft.AspNetCore.Mvc;
using ExpenseTracker.Api.DTOs;
using ExpenseTracker.Api.Services;

namespace ExpenseTracker.Api.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public ReportsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [HttpGet]
    public ActionResult<ReportResponse> GetReport(
        [FromQuery] DateOnly? dateFrom,
        [FromQuery] DateOnly? dateTo)
    {
        if (dateFrom is null || dateTo is null)
            return BadRequest(new { error = "VALIDATION_ERROR", message = "Both dateFrom and dateTo are required.", details = (object?)null });

        var result = _analyticsService.GetReport(dateFrom.Value, dateTo.Value);
        return Ok(result);
    }
}
