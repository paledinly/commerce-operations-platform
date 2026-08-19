using Dapper;
using MySqlConnector;

namespace Commerce.Operations.Api.Auth;

public static class DatabaseBootstrap
{
    public static async Task MigrateAsync(string connectionString, string contentRoot)
    {
        await using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync("CREATE TABLE IF NOT EXISTS schema_migrations (version VARCHAR(255) PRIMARY KEY, applied_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP)");
        foreach (var path in Directory.GetFiles(Path.Combine(contentRoot, "Migrations"), "*.sql").Order())
        {
            var version = Path.GetFileName(path);
            if (await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM schema_migrations WHERE version=@version", new { version }) != 0) continue;
            await connection.ExecuteAsync(await File.ReadAllTextAsync(path));
            await connection.ExecuteAsync("INSERT INTO schema_migrations(version) VALUES (@version)", new { version });
        }
    }

    public static async Task SeedOperatorAsync(string connectionString, IConfiguration configuration, PasswordService passwords)
    {
        var email = configuration["InitialAdmin:Email"];
        var password = configuration["InitialAdmin:Password"];
        var displayName = configuration["InitialAdmin:DisplayName"] ?? "Local Administrator";
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("InitialAdmin email and password are required.");
        if (password.Length < 12) throw new InvalidOperationException("InitialAdmin password must contain at least 12 characters.");

        await using var connection = new MySqlConnection(connectionString);
        await connection.ExecuteAsync("""
            INSERT INTO operator_accounts(email, display_name, role, password_hash, is_active)
            VALUES (@email, @displayName, 'ADMIN', @passwordHash, TRUE)
            ON DUPLICATE KEY UPDATE display_name = VALUES(display_name)
            """, new { email = email.Trim().ToLowerInvariant(), displayName, passwordHash = passwords.Hash(password) });
    }
}

