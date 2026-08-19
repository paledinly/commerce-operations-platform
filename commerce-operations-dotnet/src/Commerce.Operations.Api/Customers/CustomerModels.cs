namespace Commerce.Operations.Api.Customers;

public static class CustomerStatuses
{
    public const string Active = "ACTIVE";
    public const string Suspended = "SUSPENDED";
    public const string Withdrawn = "WITHDRAWN";
    public static readonly string[] All = [Active, Suspended, Withdrawn];
}

public sealed record CustomerResponse(long Id, string Email, string Name, string Phone, string Status, DateTime CreatedAt, DateTime UpdatedAt);
public sealed record CustomerListResponse(IReadOnlyList<CustomerResponse> Items, int Page, int PageSize, long TotalCount);
public sealed record CreateCustomerRequest(string Email, string Name, string Phone, string Status = CustomerStatuses.Active);
public sealed record UpdateCustomerRequest(string Email, string Name, string Phone, string Status);
public sealed record ChangeCustomerStatusRequest(string Status);
public sealed record CustomerQuery(string? Search, string? Status, int Page = 1, int PageSize = 20, string SortBy = "createdAt", string SortDirection = "desc");

public static class CustomerNormalization
{
    public static string Email(string value) => value.Trim().ToLowerInvariant();
    public static string Phone(string value)
    {
        var trimmed = value.Trim();
        var prefix = trimmed.StartsWith('+') ? "+" : string.Empty;
        return prefix + new string(trimmed.Where(char.IsDigit).ToArray());
    }
}

