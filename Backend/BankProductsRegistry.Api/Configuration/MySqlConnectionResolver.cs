using MySqlConnector;
using Microsoft.EntityFrameworkCore;

namespace BankProductsRegistry.Api.Configuration;

public static class MySqlConnectionResolver
{
    private const string DefaultServerVersion = "8.0.36-mysql";

    public static string ResolveConnectionString(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            return LooksLikeConnectionUrl(connectionString)
                ? BuildFromConnectionUrl(connectionString)
                : NormalizeConnectionString(connectionString);
        }

        var mysqlUrl = configuration["MYSQL_URL"] ?? configuration["MYSQL_PUBLIC_URL"] ?? configuration["DATABASE_URL"];
        if (!string.IsNullOrWhiteSpace(mysqlUrl))
        {
            return BuildFromConnectionUrl(mysqlUrl);
        }

        var host = configuration["MYSQLHOST"] ?? configuration["MYSQL_HOST"] ?? configuration["DB_HOST"];
        var port = configuration["MYSQLPORT"] ?? configuration["MYSQL_PORT"] ?? configuration["DB_PORT"] ?? "3306";
        var database = configuration["MYSQLDATABASE"] ?? configuration["MYSQL_DATABASE"] ?? configuration["DB_NAME"] ?? "bank_products_registry_db";
        var username = configuration["MYSQLUSER"] ?? configuration["MYSQL_USER"] ?? configuration["DB_USER"] ?? "bank_user";
        var password = configuration["MYSQLPASSWORD"] ?? configuration["MYSQL_PASSWORD"] ?? configuration["DB_PASSWORD"];
        var sslMode = configuration["MYSQL_SSL_MODE"];

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "No se configuro la conexion a MySQL. Define ConnectionStrings__DefaultConnection, MYSQL_URL o las variables MYSQL*.");
        }

        var builder = new MySqlConnectionStringBuilder
        {
            Server = host,
            Port = uint.Parse(port),
            Database = database,
            UserID = username,
            Password = password,
            SslMode = ResolveSslMode(host, sslMode)
        };

        return NormalizeConnectionString(builder.ConnectionString);
    }

    private static string BuildFromConnectionUrl(string connectionUrl)
    {
        var uri = new Uri(connectionUrl);
        var userInfo = uri.UserInfo.Split(':', 2);
        var username = Uri.UnescapeDataString(userInfo[0]);
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;

        var builder = new MySqlConnectionStringBuilder
        {
            Server = uri.Host,
            Port = (uint)(uri.Port > 0 ? uri.Port : 3306),
            Database = uri.AbsolutePath.TrimStart('/'),
            UserID = username,
            Password = password,
            SslMode = ResolveSslMode(uri.Host, null)
        };

        return NormalizeConnectionString(builder.ConnectionString);
    }

    private static string NormalizeConnectionString(string connectionString)
    {
        var builder = new MySqlConnectionStringBuilder(connectionString);

        if (builder.Port == 0)
        {
            builder.Port = 3306;
        }

        if (builder.SslMode == MySqlSslMode.Preferred && IsLocalHost(builder.Server))
        {
            builder.SslMode = MySqlSslMode.None;
        }

        if (builder.SslMode == MySqlSslMode.None)
        {
            builder.AllowPublicKeyRetrieval = true;
        }

        return builder.ConnectionString;
    }

    private static bool LooksLikeConnectionUrl(string value) =>
        value.StartsWith("mysql://", StringComparison.OrdinalIgnoreCase) ||
        value.StartsWith("mysqls://", StringComparison.OrdinalIgnoreCase);

    private static MySqlSslMode ResolveSslMode(string host, string? configuredSslMode)
    {
        if (!string.IsNullOrWhiteSpace(configuredSslMode) &&
            Enum.TryParse<MySqlSslMode>(configuredSslMode, true, out var parsedSslMode))
        {
            return parsedSslMode;
        }

        if (IsLocalHost(host))
            return MySqlSslMode.None;

        return host.Contains("rlwy.net", StringComparison.OrdinalIgnoreCase)
            ? MySqlSslMode.Required
            : MySqlSslMode.Preferred;
    }

    private static bool IsLocalHost(string host) =>
        host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
        host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase) ||
        host.Equals("host.docker.internal", StringComparison.OrdinalIgnoreCase);

    public static ServerVersion ResolveServerVersion(IConfiguration configuration)
    {
        var configuredVersion = configuration["MYSQL_SERVER_VERSION"];

        return string.IsNullOrWhiteSpace(configuredVersion)
            ? ServerVersion.Parse(DefaultServerVersion)
            : ServerVersion.Parse(configuredVersion);
    }
}
