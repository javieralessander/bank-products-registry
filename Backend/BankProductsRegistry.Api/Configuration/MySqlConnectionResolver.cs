using MySqlConnector;
using Microsoft.EntityFrameworkCore;

namespace BankProductsRegistry.Api.Configuration;

public static class MySqlConnectionResolver
{
    private const string DefaultServerVersion = "8.0.36-mysql";

    public static string ResolveConnectionString(IConfiguration configuration)
    {
        // Prioriza variables de entorno explicitas para no quedar atado al localhost de appsettings.
        var environmentConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        if (!string.IsNullOrWhiteSpace(environmentConnectionString))
        {
            return LooksLikeConnectionUrl(environmentConnectionString)
                ? BuildFromConnectionUrl(environmentConnectionString)
                : NormalizeConnectionString(environmentConnectionString);
        }

        var isRailwayRuntime = !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("RAILWAY_PROJECT_ID")) ||
                               !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("RAILWAY_SERVICE_ID")) ||
                               !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("RAILWAY_ENVIRONMENT"));
        var mysqlUrl = ResolveEnvironmentMySqlUrl(isRailwayRuntime);
        if (!string.IsNullOrWhiteSpace(mysqlUrl))
        {
            return BuildFromConnectionUrl(mysqlUrl);
        }

        var host = ResolveEnvironmentValue("MYSQLHOST", "MYSQL_HOST", "DB_HOST");
        var port = ResolveEnvironmentValue("MYSQLPORT", "MYSQL_PORT", "DB_PORT") ?? "3306";
        var database = ResolveEnvironmentValue("MYSQLDATABASE", "MYSQL_DATABASE", "DB_NAME") ?? "bank_products_registry_db";
        var username = ResolveEnvironmentValue("MYSQLUSER", "MYSQL_USER", "DB_USER") ?? "bank_user";
        var password = ResolveEnvironmentValue("MYSQLPASSWORD", "MYSQL_PASSWORD", "DB_PASSWORD");
        var sslMode = ResolveEnvironmentValue("MYSQL_SSL_MODE");

        if (!string.IsNullOrWhiteSpace(host) && !string.IsNullOrWhiteSpace(password))
        {
            var environmentBuilder = new MySqlConnectionStringBuilder
            {
                Server = host,
                Port = uint.Parse(port),
                Database = database,
                UserID = username,
                Password = password,
                SslMode = ResolveSslMode(host, sslMode)
            };

            return NormalizeConnectionString(environmentBuilder.ConnectionString);
        }

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            return LooksLikeConnectionUrl(connectionString)
                ? BuildFromConnectionUrl(connectionString)
                : NormalizeConnectionString(connectionString);
        }

        mysqlUrl = configuration["MYSQL_URL"] ?? configuration["MYSQL_PUBLIC_URL"] ?? configuration["DATABASE_URL"];
        if (!string.IsNullOrWhiteSpace(mysqlUrl))
        {
            return BuildFromConnectionUrl(mysqlUrl);
        }

        host = configuration["MYSQLHOST"] ?? configuration["MYSQL_HOST"] ?? configuration["DB_HOST"];
        port = configuration["MYSQLPORT"] ?? configuration["MYSQL_PORT"] ?? configuration["DB_PORT"] ?? "3306";
        database = configuration["MYSQLDATABASE"] ?? configuration["MYSQL_DATABASE"] ?? configuration["DB_NAME"] ?? "bank_products_registry_db";
        username = configuration["MYSQLUSER"] ?? configuration["MYSQL_USER"] ?? configuration["DB_USER"] ?? "bank_user";
        password = configuration["MYSQLPASSWORD"] ?? configuration["MYSQL_PASSWORD"] ?? configuration["DB_PASSWORD"];
        sslMode = configuration["MYSQL_SSL_MODE"];

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

    private static string? ResolveEnvironmentMySqlUrl(bool isRailwayRuntime)
    {
        var mysqlUrl = Environment.GetEnvironmentVariable("MYSQL_URL");
        var mysqlPublicUrl = Environment.GetEnvironmentVariable("MYSQL_PUBLIC_URL");
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

        if (!isRailwayRuntime && !string.IsNullOrWhiteSpace(mysqlPublicUrl))
        {
            return mysqlPublicUrl;
        }

        return mysqlUrl ?? mysqlPublicUrl ?? databaseUrl;
    }

    private static string? ResolveEnvironmentValue(params string[] keys)
    {
        foreach (var key in keys)
        {
            var value = Environment.GetEnvironmentVariable(key);
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
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
