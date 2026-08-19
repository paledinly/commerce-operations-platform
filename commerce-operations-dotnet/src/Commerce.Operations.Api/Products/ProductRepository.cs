using Dapper;
using MySqlConnector;

namespace Commerce.Operations.Api.Products;

public sealed class ProductRepository(IConfiguration configuration)
{
    private string ConnectionString => configuration.GetConnectionString("Operations")
        ?? throw new InvalidOperationException("Operations connection string is required.");

    public async Task<ProductListResponse> SearchAsync(ProductQuery query)
    {
        var where = new List<string>();
        if (!string.IsNullOrWhiteSpace(query.Search)) where.Add("(sku LIKE @term OR name LIKE @term)");
        if (!string.IsNullOrWhiteSpace(query.Status)) where.Add("status = @status");
        var predicate = where.Count == 0 ? string.Empty : "WHERE " + string.Join(" AND ", where);
        var orderColumn = query.SortBy switch { "sku" => "sku", "name" => "name", "price" => "price", "status" => "status", "updatedAt" => "updated_at", _ => "created_at" };
        var direction = query.SortDirection.Equals("asc", StringComparison.OrdinalIgnoreCase) ? "ASC" : "DESC";
        var parameters = new { term = $"%{query.Search?.Trim()}%", status = query.Status?.ToUpperInvariant(), offset = (query.Page - 1) * query.PageSize, query.PageSize };
        await using var connection = new MySqlConnection(ConnectionString);
        var items = (await connection.QueryAsync<ProductResponse>($"""
            SELECT id, sku, name, price, status, created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM products {predicate} ORDER BY {orderColumn} {direction}, id DESC LIMIT @PageSize OFFSET @offset
            """, parameters)).AsList();
        var total = await connection.ExecuteScalarAsync<long>($"SELECT COUNT(*) FROM products {predicate}", parameters);
        return new ProductListResponse(items, query.Page, query.PageSize, total);
    }

    public async Task<ProductResponse?> GetAsync(long id)
    {
        await using var connection = new MySqlConnection(ConnectionString);
        return await connection.QuerySingleOrDefaultAsync<ProductResponse>("SELECT id, sku, name, price, status, created_at AS CreatedAt, updated_at AS UpdatedAt FROM products WHERE id=@id", new { id });
    }

    public async Task<long> CreateAsync(CreateProductRequest request)
    {
        await using var connection = new MySqlConnection(ConnectionString);
        await connection.OpenAsync();
        await using var command = new MySqlCommand("INSERT INTO products(sku,name,price,status) VALUES(@sku,@name,@price,@status)", connection);
        command.Parameters.AddWithValue("@sku", request.Sku.Trim().ToUpperInvariant());
        command.Parameters.AddWithValue("@name", request.Name.Trim());
        command.Parameters.AddWithValue("@price", request.Price);
        command.Parameters.AddWithValue("@status", request.Status.ToUpperInvariant());
        await command.ExecuteNonQueryAsync();
        return command.LastInsertedId;
    }

    public async Task<bool> UpdateAsync(long id, UpdateProductRequest request)
    {
        await using var connection = new MySqlConnection(ConnectionString);
        return await connection.ExecuteAsync("UPDATE products SET sku=@sku,name=@name,price=@price,status=@status WHERE id=@id", new { id, sku = request.Sku.Trim().ToUpperInvariant(), name = request.Name.Trim(), request.Price, status = request.Status.ToUpperInvariant() }) > 0;
    }

    public async Task<bool> ChangeStatusAsync(long id, string status)
    {
        await using var connection = new MySqlConnection(ConnectionString);
        return await connection.ExecuteAsync("UPDATE products SET status=@status WHERE id=@id", new { id, status = status.ToUpperInvariant() }) > 0;
    }

}
