using Microsoft.Data.SqlClient;

namespace AlfaBank.Core.Data;

/// <summary>
/// Формирует строку подключения к базе данных SQL Server (LocalDB).
/// </summary>
public static class ConnectionStringProvider
{
    /// <summary>
    /// Имя переменной окружения, в которой можно переопределить строку подключения.
    /// </summary>
    public const string OverrideVariableName = "ALFABANK_CONNECTION_STRING";

    /// <summary>
    /// Имя экземпляра локальной базы данных Microsoft SQL Server.
    /// </summary>
    public const string LocalDbDataSource = @"(localdb)\MSSQLLocalDB";

    /// <summary>
    /// Имя базы данных проекта.
    /// </summary>
    public const string DatabaseName = "AlfaBankDb";

    /// <summary>
    /// Тайм-аут подключения к серверу в секундах.
    /// </summary>
    public const int ConnectTimeoutSeconds = 30;

    /// <summary>
    /// Возвращает строку подключения: из переменной окружения, если она задана,
    /// иначе — стандартную строку подключения к локальной базе данных.
    /// </summary>
    /// <returns>Строка подключения к базе данных.</returns>
    public static string GetConnectionString()
    {
        var overriddenConnectionString = Environment.GetEnvironmentVariable(OverrideVariableName);

        return string.IsNullOrWhiteSpace(overriddenConnectionString)
            ? BuildLocalDbConnectionString()
            : overriddenConnectionString;
    }

    /// <summary>
    /// Собирает строку подключения к локальной базе данных с проверкой подлинности Windows.
    /// </summary>
    /// <returns>Строка подключения к экземпляру LocalDB.</returns>
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
