using MySqlConnector;

namespace AkarangaBank.Web.Data;

internal static class RailwayMySqlConfiguration
{
    public static string GetConnectionString(IConfiguration configuration)
    {
        var databaseUrl = configuration["MYSQL_URL"];
        if (string.IsNullOrWhiteSpace(databaseUrl))
            databaseUrl = configuration["MARIADB_PRIVATE_URL"];
        if (!string.IsNullOrWhiteSpace(databaseUrl))
            return NormalizeConnectionString(databaseUrl);

        var host = FirstConfiguredValue(configuration, "MYSQLHOST", "MARIADB_HOST");
        var database = FirstConfiguredValue(configuration, "MYSQLDATABASE", "MARIADB_DATABASE");
        var user = FirstConfiguredValue(configuration, "MYSQLUSER", "MARIADB_USER");
        var password = FirstConfiguredValue(configuration, "MYSQLPASSWORD", "MARIADB_PASSWORD");

        if (string.IsNullOrWhiteSpace(host)
            || string.IsNullOrWhiteSpace(database)
            || string.IsNullOrWhiteSpace(user)
            || string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "La chaîne de connexion 'AkarangaBank' est absente. Dans Railway, ajoutez au service web " +
                "ConnectionStrings__AkarangaBank avec la référence vers MYSQL_URL ou MARIADB_PRIVATE_URL, " +
                "ou liez les variables MYSQL* ou MARIADB_* depuis le service de base de données.");
        }

        var connection = new MySqlConnectionStringBuilder
        {
            Server = host,
            Database = database,
            UserID = user,
            Password = password
        };

        var databasePort = FirstConfiguredValue(configuration, "MYSQLPORT", "MARIADB_PORT");
        if (!string.IsNullOrWhiteSpace(databasePort))
        {
            if (!uint.TryParse(databasePort, out var port) || port == 0)
                throw new InvalidOperationException("Le port Railway MYSQLPORT ou MARIADB_PORT doit être valide.");

            connection.Port = port;
        }

        return connection.ConnectionString;
    }

    private static string? FirstConfiguredValue(IConfiguration configuration, string primaryKey, string fallbackKey)
    {
        var value = configuration[primaryKey];
        return string.IsNullOrWhiteSpace(value) ? configuration[fallbackKey] : value;
    }

    public static string NormalizeConnectionString(string connectionString)
    {
        if (!connectionString.StartsWith("mysql://", StringComparison.OrdinalIgnoreCase)
            && !connectionString.StartsWith("mariadb://", StringComparison.OrdinalIgnoreCase))
            return connectionString;

        if (!Uri.TryCreate(connectionString, UriKind.Absolute, out var uri)
            || string.IsNullOrWhiteSpace(uri.Host))
        {
            throw new InvalidOperationException("La variable MYSQL_URL Railway n'est pas une URL MySQL valide.");
        }

        var credentials = uri.UserInfo.Split(':', 2);
        var database = Uri.UnescapeDataString(uri.AbsolutePath.Trim('/'));
        if (credentials.Length != 2 || string.IsNullOrWhiteSpace(database))
        {
            throw new InvalidOperationException(
                "La variable MYSQL_URL Railway doit contenir un utilisateur, un mot de passe et une base de données.");
        }

        var connection = new MySqlConnectionStringBuilder
        {
            Server = uri.Host,
            Port = uri.Port > 0 ? (uint)uri.Port : 3306,
            Database = database,
            UserID = Uri.UnescapeDataString(credentials[0]),
            Password = Uri.UnescapeDataString(credentials[1])
        };

        return connection.ConnectionString;
    }
}
