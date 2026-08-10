using System.Text;
using IMGR.Employee.Api.Configuration;
using IMGR.Employee.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(options => options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ");
builder.Logging.AddDebug();

var employeeSettings = builder.Configuration.GetSection("Employee").Get<EmployeeSettings>()
    ?? throw new InvalidOperationException("The Employee configuration section is required.");
var authenticationSettings = builder.Configuration.GetSection("Authentication").Get<AuthenticationSettings>()
    ?? throw new InvalidOperationException("The Authentication configuration section is required.");
authenticationSettings.Validate();

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
    };
});
builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.AddEmployeeService(options =>
{
    options.ControlDatabase.ConnectionString = employeeSettings.ControlDatabase.ConnectionString;
    options.ControlDatabase.LegacyEncryptionKey = employeeSettings.ControlDatabase.LegacyEncryptionKey;
    options.ControlDatabase.RequireImgrEnabled = employeeSettings.ControlDatabase.RequireImgrEnabled;
    options.ControlDatabase.TenantEncrypt = employeeSettings.ControlDatabase.TenantEncrypt;
    options.ControlDatabase.TenantTrustServerCertificate =
        employeeSettings.ControlDatabase.TenantTrustServerCertificate;
    options.ControlDatabase.TenantConnectTimeoutSeconds =
        employeeSettings.ControlDatabase.TenantConnectTimeoutSeconds;
    options.ControlDatabase.TenantCompatibilityLevel =
        employeeSettings.ControlDatabase.TenantCompatibilityLevel;
    foreach (var tenant in employeeSettings.Tenants)
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
            ValidIssuer = authenticationSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = authenticationSettings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(authenticationSettings.SigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });
builder.Services.AddAuthorization();

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
        Title = "iMGR Employee API",
        Version = "v1",
        Description = "Employee and organization master-data boundary for the iMGR modernization program."
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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false }).AllowAnonymous();
app.Run();

public partial class Program;
