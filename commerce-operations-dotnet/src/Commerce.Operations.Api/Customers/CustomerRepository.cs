using Dapper;
using MySqlConnector;

namespace Commerce.Operations.Api.Customers;

public sealed class CustomerRepository(IConfiguration configuration)
{
    private string ConnectionString => configuration.GetConnectionString("Operations") ?? throw new InvalidOperationException("Operations connection string is required.");
    private const string SelectColumns = "id, email, name, phone, status, created_at AS CreatedAt, updated_at AS UpdatedAt";

    public async Task<CustomerListResponse> SearchAsync(CustomerQuery query)
    {
        var where = new List<string>();
        if (!string.IsNullOrWhiteSpace(query.Search)) where.Add("(email LIKE @term OR name LIKE @term OR phone LIKE @phoneTerm)");
        if (!string.IsNullOrWhiteSpace(query.Status)) where.Add("status=@status");
        var predicate = where.Count == 0 ? string.Empty : "WHERE " + string.Join(" AND ", where);
        var orderColumn = query.SortBy switch { "email" => "email", "name" => "name", "status" => "status", "updatedAt" => "updated_at", _ => "created_at" };
        var direction = query.SortDirection.Equals("asc", StringComparison.OrdinalIgnoreCase) ? "ASC" : "DESC";
        var parameters = new { term = $"%{query.Search?.Trim()}%", phoneTerm = $"%{CustomerNormalization.Phone(query.Search ?? string.Empty)}%", status = query.Status?.ToUpperInvariant(), offset = (query.Page - 1) * query.PageSize, query.PageSize };
        await using var connection = new MySqlConnection(ConnectionString);
        var items = (await connection.QueryAsync<CustomerResponse>($"SELECT {SelectColumns} FROM customers {predicate} ORDER BY {orderColumn} {direction}, id DESC LIMIT @PageSize OFFSET @offset", parameters)).AsList();
        var total = await connection.ExecuteScalarAsync<long>($"SELECT COUNT(*) FROM customers {predicate}", parameters);
        return new CustomerListResponse(items, query.Page, query.PageSize, total);
    }

    public async Task<CustomerResponse?> GetAsync(long id)
    {
        await using var connection = new MySqlConnection(ConnectionString);
        return await connection.QuerySingleOrDefaultAsync<CustomerResponse>($"SELECT {SelectColumns} FROM customers WHERE id=@id", new { id });
    }

    public async Task<long> CreateAsync(CreateCustomerRequest request)
    {
        await using var connection = new MySqlConnection(ConnectionString);
        await connection.OpenAsync();
        await using var command = new MySqlCommand("INSERT INTO customers(email,name,phone,status) VALUES(@email,@name,@phone,@status)", connection);
        command.Parameters.AddWithValue("@email", CustomerNormalization.Email(request.Email));
        command.Parameters.AddWithValue("@name", request.Name.Trim());
        command.Parameters.AddWithValue("@phone", CustomerNormalization.Phone(request.Phone));
        command.Parameters.AddWithValue("@status", request.Status.ToUpperInvariant());
        await command.ExecuteNonQueryAsync();
        return command.LastInsertedId;
    }

    public async Task<bool> UpdateAsync(long id, UpdateCustomerRequest request)
    {
        await using var connection = new MySqlConnection(ConnectionString);
        return await connection.ExecuteAsync("UPDATE customers SET email=@email,name=@name,phone=@phone,status=@status WHERE id=@id", new { id, email = CustomerNormalization.Email(request.Email), name = request.Name.Trim(), phone = CustomerNormalization.Phone(request.Phone), status = request.Status.ToUpperInvariant() }) > 0;
    }

    public async Task<bool> ChangeStatusAsync(long id, string status)
    {
        await using var connection = new MySqlConnection(ConnectionString);
        return await connection.ExecuteAsync("UPDATE customers SET status=@status WHERE id=@id", new { id, status = status.ToUpperInvariant() }) > 0;
    }
}

