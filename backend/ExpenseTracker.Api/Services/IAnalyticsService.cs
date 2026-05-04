using ExpenseTracker.Api.DTOs;

namespace ExpenseTracker.Api.Services;

public interface IAnalyticsService
{
    BalanceResponse GetBalance(DateOnly? from, DateOnly? to);
    ReportResponse GetReport(DateOnly from, DateOnly to);
}
