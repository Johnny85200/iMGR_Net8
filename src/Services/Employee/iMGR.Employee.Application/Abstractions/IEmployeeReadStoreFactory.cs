namespace IMGR.Employee.Application.Abstractions;

public interface IEmployeeReadStoreFactory
{
    ValueTask<IEmployeeReadStore?> CreateAsync(string tenantCode, CancellationToken cancellationToken);
}
