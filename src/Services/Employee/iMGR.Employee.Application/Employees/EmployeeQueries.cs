using IMGR.Employee.Domain.Employees;

namespace IMGR.Employee.Application.Employees;

public sealed record EmployeeSearchQuery(
    string TenantCode,
    string? Search,
    EmployeeStatus? Status,
    DateOnly AsOfDate,
    int Page,
    int PageSize);

public sealed record EmployeePage(
    IReadOnlyList<EmployeeSummary> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public int TotalPages => TotalCount == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
