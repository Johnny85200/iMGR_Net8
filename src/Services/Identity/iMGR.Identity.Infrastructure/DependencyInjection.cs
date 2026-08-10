using IMGR.Identity.Application.Abstractions;
using IMGR.Identity.Application.Authentication;
using IMGR.Identity.Infrastructure.Persistence;
using IMGR.Identity.Infrastructure.Security;
using IMGR.Identity.Infrastructure.Tenancy;
using Microsoft.Extensions.DependencyInjection;

namespace IMGR.Identity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityService(
        this IServiceCollection services,
        Action<IdentityInfrastructureOptions> configure)
    {
        var options = new IdentityInfrastructureOptions();
        configure(options);

        options.Jwt.Validate();
        options.ControlDatabase.Validate();

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IPasswordHasher, CompatiblePasswordHasher>();
        services.AddSingleton(new AuthenticationPolicy
        {
            UpgradeLegacyPasswordHashes = options.UpgradeLegacyPasswordHashes
        });
        services.AddSingleton<ITokenIssuer>(provider =>
            new JwtTokenIssuer(options.Jwt, provider.GetRequiredService<TimeProvider>()));
        services.AddSingleton<ITenantRegistry>(options.ControlDatabase.IsConfigured
            ? new ControlDatabaseTenantRegistry(options.ControlDatabase)
            : new ConfiguredTenantRegistry(options.Tenants));
        services.AddSingleton<IIdentityStoreFactory, EfIdentityStoreFactory>();
        services.AddScoped<LoginService>();

        return services;
    }
}
