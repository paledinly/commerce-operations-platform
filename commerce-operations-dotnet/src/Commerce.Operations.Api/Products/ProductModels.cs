namespace Commerce.Operations.Api.Products;

public static class ProductStatuses
{
    public const string Active = "ACTIVE";
    public const string Inactive = "INACTIVE";
    public static readonly string[] All = [Active, Inactive];
}

public sealed record ProductResponse(long Id, string Sku, string Name, decimal Price, string Status, DateTime CreatedAt, DateTime UpdatedAt);
public sealed record ProductListResponse(IReadOnlyList<ProductResponse> Items, int Page, int PageSize, long TotalCount);
public sealed record CreateProductRequest(string Sku, string Name, decimal Price, string Status = ProductStatuses.Active);
public sealed record UpdateProductRequest(string Sku, string Name, decimal Price, string Status);
public sealed record ChangeProductStatusRequest(string Status);
public sealed record ProductQuery(string? Search, string? Status, int Page = 1, int PageSize = 20, string SortBy = "createdAt", string SortDirection = "desc");

