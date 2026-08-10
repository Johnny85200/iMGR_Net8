using System.Text;
using System.Threading.RateLimiting;
using IMGR.Identity.Api.Configuration;
using IMGR.Identity.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(options => options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ");
builder.Logging.AddDebug();

var identitySettings = builder.Configuration.GetSection("Identity").Get<IdentitySettings>()
    ?? throw new InvalidOperationException("The Identity configuration section is required.");

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
    };
});
builder.Services.AddControllers();
builder.Services.AddHealthChecks();

builder.Services.AddIdentityService(options =>
{
    options.UpgradeLegacyPasswordHashes = identitySettings.UpgradeLegacyPasswordHashes;
    options.ControlDatabase.ConnectionString = identitySettings.ControlDatabase.ConnectionString;
    options.ControlDatabase.LegacyEncryptionKey = identitySettings.ControlDatabase.LegacyEncryptionKey;
    options.ControlDatabase.RequireImgrEnabled = identitySettings.ControlDatabase.RequireImgrEnabled;
    options.ControlDatabase.TenantEncrypt = identitySettings.ControlDatabase.TenantEncrypt;
    options.ControlDatabase.TenantTrustServerCertificate =
        identitySettings.ControlDatabase.TenantTrustServerCertificate;
    options.ControlDatabase.TenantConnectTimeoutSeconds =
        identitySettings.ControlDatabase.TenantConnectTimeoutSeconds;
    options.Jwt.Issuer = identitySettings.Jwt.Issuer;
    options.Jwt.Audience = identitySettings.Jwt.Audience;
    options.Jwt.SigningKey = identitySettings.Jwt.SigningKey;
    options.Jwt.AccessTokenMinutes = identitySettings.Jwt.AccessTokenMinutes;

    foreach (var tenant in identitySettings.Tenants)
    {
        options.Tenants[tenant.Key] = tenant.Value;
    }
});

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = identitySettings.Jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = identitySettings.Jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(identitySettings.Jwt.SigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("login", limiter =>
    {
        limiter.PermitLimit = 10;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
        limiter.AutoReplenishment = true;
    });
});

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
        }
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "iMGR Identity API",
        Version = "v1",
        Description = "Authentication boundary for the iMGR modernization program."
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
        }] = Array.Empty<string>()
    });
});

var app = builder.Build();

app.UseExceptionHandler();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("frontend");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
}).AllowAnonymous();

app.Run();

public partial class Program;
