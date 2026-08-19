using Dapper;
using MySqlConnector;

namespace Commerce.Operations.Api.Auth;

public sealed class OperatorRepository(IConfiguration configuration)
{
    public async Task<OperatorAccount?> FindByEmailAsync(string email)
    {
        await using var connection = new MySqlConnection(configuration.GetConnectionString("Operations"));
        return await connection.QuerySingleOrDefaultAsync<OperatorAccount>("""
            SELECT id, email, display_name AS DisplayName, role, password_hash AS PasswordHash, is_active AS IsActive
            FROM operator_accounts WHERE email = @email LIMIT 1
            """, new { email = email.Trim().ToLowerInvariant() });
    }
}

