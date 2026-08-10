using IMGR.Employee.Application.Abstractions;
using IMGR.Employee.Domain.Employees;
using IMGR.Employee.Domain.Profiles;

namespace IMGR.Employee.Application.Employees;

public sealed class EmployeeDirectoryService(IEmployeeReadStoreFactory storeFactory)
{
    public async Task<EmployeePage?> SearchAsync(
        string tenantCode,
        string? search,
        EmployeeStatus? status,
        DateOnly asOfDate,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        ValidateTenantCode(tenantCode);
        if (page < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(page), "Page must be at least 1.");
        }

        if (pageSize is < 1 or > 200)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be between 1 and 200.");
        }

        await using var store = await storeFactory.CreateAsync(tenantCode, cancellationToken);
        if (store is null)
        {
            return null;
        }

        var query = new EmployeeSearchQuery(
            tenantCode.Trim().ToUpperInvariant(),
            string.IsNullOrWhiteSpace(search) ? null : search.Trim(),
            status,
            asOfDate,
            page,
            pageSize);
        return await store.SearchAsync(query, cancellationToken);
    }

    public async Task<EmployeeProfile?> GetAsync(
        string tenantCode,
        int employeeId,
        DateOnly asOfDate,
        CancellationToken cancellationToken)
    {
        ValidateTenantCode(tenantCode);
        if (employeeId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(employeeId));
        }

        await using var store = await storeFactory.CreateAsync(tenantCode, cancellationToken);
        return store is null
            ? null
            : await store.GetAsync(employeeId, asOfDate, cancellationToken);
    }

    public async Task<IReadOnlyList<OrganizationUnit>?> GetOrganizationsAsync(
        string tenantCode,
        int? companyId,
        int? levelId,
        CancellationToken cancellationToken)
    {
        ValidateTenantCode(tenantCode);
        await using var store = await storeFactory.CreateAsync(tenantCode, cancellationToken);
        return store is null
            ? null
            : await store.GetOrganizationsAsync(companyId, levelId, cancellationToken);
    }

    private static void ValidateTenantCode(string tenantCode)
    {
        if (string.IsNullOrWhiteSpace(tenantCode))
        {
            throw new ArgumentException("A tenant code is required.", nameof(tenantCode));
        }
    }
}
