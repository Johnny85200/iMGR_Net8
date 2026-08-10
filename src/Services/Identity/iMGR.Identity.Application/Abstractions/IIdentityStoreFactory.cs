namespace IMGR.Identity.Application.Abstractions;

public interface IIdentityStoreFactory
{
    ValueTask<IIdentityStore?> OpenAsync(string tenantCode, CancellationToken cancellationToken);
}

