namespace Commerce.Operations.Api.Auth;

public sealed record LoginRequest(string Email, string Password);
public sealed record LoginResponse(string AccessToken, DateTime ExpiresAtUtc, OperatorProfile Operator);
public sealed record OperatorProfile(long Id, string Email, string DisplayName, string Role);
public sealed record OperatorAccount(long Id, string Email, string DisplayName, string Role, string PasswordHash, bool IsActive);

public sealed class JwtOptions
{
    public string Secret { get; init; } = string.Empty;
    public string Issuer { get; init; } = "commerce-operations-api";
    public string Audience { get; init; } = "commerce-operations-ui";
    public int ExpiryMinutes { get; init; } = 60;
}

