using IMGR.Employee.Application.Employees;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IMGR.Employee.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/organizations")]
public sealed class OrganizationsController(EmployeeDirectoryService employeeDirectory) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(
        [FromQuery] int? companyId,
        [FromQuery] int? levelId,
        CancellationToken cancellationToken)
    {
        var tenantCode = User.FindFirst("tenant")?.Value;
        if (string.IsNullOrWhiteSpace(tenantCode))
        {
            return Forbid();
        }

        var organizations = await employeeDirectory.GetOrganizationsAsync(
            tenantCode,
            companyId,
            levelId,
            cancellationToken);
        return organizations is null
            ? Problem(statusCode: StatusCodes.Status404NotFound, title: "Tenant not found")
            : Ok(organizations);
    }
}
