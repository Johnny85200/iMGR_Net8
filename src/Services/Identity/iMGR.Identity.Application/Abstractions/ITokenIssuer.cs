using IMGR.Identity.Domain.Users;

namespace IMGR.Identity.Application.Abstractions;

public interface ITokenIssuer
{
    AccessToken Issue(User user, string tenantCode, bool passwordChangeRequired);
}

