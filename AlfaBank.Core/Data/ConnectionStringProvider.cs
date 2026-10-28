using Microsoft.Data.SqlClient;

namespace AlfaBank.Core.Data;

public static class ConnectionStringProvider
{
    public const string OverrideVariableName = "ALFABANK_CONNECTION_STRING";

    public const string LocalDbDataSource = @"(localdb)\MSSQLLocalDB";

    public const string DatabaseName = "AlfaBankDb";

    public const int ConnectTimeoutSeconds = 30;

    public static string GetConnectionString()
    {
        var overriddenConnectionString = Environment.GetEnvironmentVariable(OverrideVariableName);

        return string.IsNullOrWhiteSpace(overriddenConnectionString)
            ? BuildLocalDbConnectionString()
            : overriddenConnectionString;
    }

    private static string BuildLocalDbConnectionString()
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = LocalDbDataSource,
            InitialCatalog = DatabaseName,
            IntegratedSecurity = true,
            MultipleActiveResultSets = true,
            Encrypt = false,
            TrustServerCertificate = true,
            ConnectTimeout = ConnectTimeoutSeconds
        };

        return builder.ConnectionString;
    }
}
