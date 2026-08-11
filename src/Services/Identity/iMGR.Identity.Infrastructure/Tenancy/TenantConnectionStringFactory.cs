using IMGR.LegacyData.Control.Entities;
using Microsoft.Data.SqlClient;

namespace IMGR.Identity.Infrastructure.Tenancy;

internal sealed class TenantConnectionStringFactory(
    ControlDatabaseOptions options,
    LegacyFieldDecryptor decryptor)
{
    public string Create(
        CompanyDatabase company,
        DatabaseServer server)
    {
        var databaseType = decryptor.Decrypt(server.DBServerDBType);
        if (!string.Equals(databaseType, "MSSQL", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "The tenant database type is unsupported or its legacy encryption key is invalid.");
        }

        var dataSource = RequireValue(decryptor.Decrypt(server.DBServerLocation), "DatabaseServer.DBServerLocation");
        var schemaName = decryptor.Decrypt(company.CompanyDBSchemaName);
        var databaseName = string.IsNullOrWhiteSpace(schemaName)
            ? RequireValue(company.CompanyDBClientCode, "CompanyDatabase.CompanyDBClientCode")
            : schemaName;
        var userId = decryptor.Decrypt(server.DBServerUserID);
        var password = decryptor.Decrypt(server.DBServerPassword);

        var builder = new SqlConnectionStringBuilder
        {
            DataSource = dataSource,
            InitialCatalog = databaseName,
            Encrypt = options.TenantEncrypt,
            TrustServerCertificate = options.TenantTrustServerCertificate,
            ConnectTimeout = options.TenantConnectTimeoutSeconds,
            PersistSecurityInfo = false,
            MultipleActiveResultSets = true
        };

        if (string.IsNullOrWhiteSpace(userId))
        {
            builder.IntegratedSecurity = true;
        }
        else
        {
            builder.UserID = userId;
            builder.Password = password ?? string.Empty;
        }

        return builder.ConnectionString;
    }

    private static string RequireValue(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"{fieldName} is empty in the control database.");
        }

        return value;
    }
}

