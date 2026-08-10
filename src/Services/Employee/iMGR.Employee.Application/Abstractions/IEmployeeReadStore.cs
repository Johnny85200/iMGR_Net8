using IMGR.Employee.Domain.Employees;
using IMGR.Employee.Domain.Profiles;
using IMGR.Employee.Application.Employees;

namespace IMGR.Employee.Application.Abstractions;

public interface IEmployeeReadStore : IAsyncDisposable
{
    Task<EmployeePage> SearchAsync(EmployeeSearchQuery query, CancellationToken cancellationToken);

    Task<EmployeeProfile?> GetAsync(int employeeId, DateOnly asOfDate, CancellationToken cancellationToken);

    Task<IReadOnlyList<OrganizationUnit>> GetOrganizationsAsync(
        int? companyId,
        int? levelId,
        CancellationToken cancellationToken);
}
