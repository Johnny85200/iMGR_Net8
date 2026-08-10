using IMGR.Employee.Application.Employees;
using IMGR.Employee.Domain.Employees;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IMGR.Employee.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/employees")]
public sealed class EmployeesController(EmployeeDirectoryService employeeDirectory) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<EmployeePage>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Search(
        [FromQuery] string? search,
        [FromQuery] EmployeeStatus? status,
        [FromQuery] DateOnly? asOf,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var tenantCode = User.FindFirst("tenant")?.Value;
        if (string.IsNullOrWhiteSpace(tenantCode))
        {
            return Forbid();
        }

        var result = await employeeDirectory.SearchAsync(
            tenantCode,
            search,
            status,
            asOf ?? DateOnly.FromDateTime(DateTime.Today),
            page,
            pageSize,
            cancellationToken);
        return result is null
            ? Problem(statusCode: StatusCodes.Status404NotFound, title: "Tenant not found")
            : Ok(result);
    }

    [HttpGet("{employeeId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(
        int employeeId,
        [FromQuery] DateOnly? asOf,
        CancellationToken cancellationToken)
    {
        var tenantCode = User.FindFirst("tenant")?.Value;
        if (string.IsNullOrWhiteSpace(tenantCode))
        {
            return Forbid();
        }

        var employee = await employeeDirectory.GetAsync(
            tenantCode,
            employeeId,
            asOf ?? DateOnly.FromDateTime(DateTime.Today),
            cancellationToken);
        return employee is null
            ? Problem(statusCode: StatusCodes.Status404NotFound, title: "Employee or tenant not found")
            : Ok(employee);
    }
}
