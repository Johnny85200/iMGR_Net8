using IMGR.Identity.Infrastructure.Tenancy;
using IMGR.Database.Control.Entities;
using Microsoft.Data.SqlClient;

namespace IMGR.Identity.UnitTests.Tenancy;

public sealed class TenantResolutionTests
{
    [Theory]
    [InlineData("HWbUW/MDjbJBIyZGnnTwIhug8iWV4XTQpjh9JDmzyXM=", "sql.example.local")]
    [InlineData("JJmo+OiG8xavQyiDLSEsyg==", "tenant_user")]
    [InlineData("+lzqJmCtKOelRNSPWKp/Ow==", "tenant_password")]
    [InlineData("655WrqLeSzGndK1D8ASrdw==", "MSSQL")]
    public void Decrypt_WithLegacyRijndaelCiphertext_ReturnsPlaintext(string encrypted, string expected)
    {
        var decryptor = new LegacyFieldDecryptor("unit-test-key");

        var actual = decryptor.Decrypt(encrypted);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Decrypt_WithPlaintext_ReturnsOriginalValue()
    {
        var decryptor = new LegacyFieldDecryptor("unit-test-key");

        var actual = decryptor.Decrypt("sql01\\SQLEXPRESS");

        Assert.Equal("sql01\\SQLEXPRESS", actual);
    }

    [Fact]
    public void Create_WithEncryptedControlRecords_BuildsTenantConnectionString()
    {
        var options = new ControlDatabaseOptions
        {
            LegacyEncryptionKey = "unit-test-key",
            TenantEncrypt = true,
            TenantTrustServerCertificate = true,
            TenantConnectTimeoutSeconds = 12
        };
        var factory = new TenantConnectionStringFactory(
            options,
            new LegacyFieldDecryptor(options.LegacyEncryptionKey));
        var company = new CompanyDatabase
        {
            CompanyDBClientCode = "CLIENT1",
            CompanyDBSchemaName = "2gyHIWkxZfWip7TOuZihCg=="
        };
        var server = new DatabaseServer
        {
            DBServerDBType = "655WrqLeSzGndK1D8ASrdw==",
            DBServerLocation = "HWbUW/MDjbJBIyZGnnTwIhug8iWV4XTQpjh9JDmzyXM=",
            DBServerUserID = "JJmo+OiG8xavQyiDLSEsyg==",
            DBServerPassword = "+lzqJmCtKOelRNSPWKp/Ow=="
        };

        var connectionString = factory.Create(company, server);
        var builder = new SqlConnectionStringBuilder(connectionString);

        Assert.Equal("sql.example.local", builder.DataSource);
        Assert.Equal("TenantSchema", builder.InitialCatalog);
        Assert.Equal("tenant_user", builder.UserID);
        Assert.Equal("tenant_password", builder.Password);
        Assert.True(builder.Encrypt);
        Assert.True(builder.TrustServerCertificate);
        Assert.Equal(12, builder.ConnectTimeout);
    }

    [Fact]
    public async Task ConfiguredRegistry_IsCaseInsensitiveFallback()
    {
        var registry = new ConfiguredTenantRegistry(new Dictionary<string, TenantDatabaseOptions>
        {
            ["hrone3"] = new() { ConnectionString = "Server=sql01;Database=hrone3" }
        });

        var tenant = await registry.ResolveAsync(" HRONE3 ", CancellationToken.None);

        Assert.NotNull(tenant);
        Assert.Equal("hrone3", tenant.Code);
        Assert.Equal("Server=sql01;Database=hrone3", tenant.ConnectionString);
    }
}
