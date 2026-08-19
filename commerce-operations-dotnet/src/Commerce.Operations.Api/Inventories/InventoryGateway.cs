using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Commerce.Operations.Api.Inventories;

public sealed class InventoryGateway(HttpClient client)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    public Task<InventoryPage> ListAsync(int page, int pageSize) => SendAsync<InventoryPage>(HttpMethod.Get, $"/internal/inventories?page={page}&pageSize={pageSize}");
    public Task<IReadOnlyList<InventoryMovement>> MovementsAsync(long productId) => SendAsync<IReadOnlyList<InventoryMovement>>(HttpMethod.Get, $"/internal/inventories/{productId}/movements");
    public Task<InventoryRecord> CreateAsync(CreateInventoryRequest request) => SendAsync<InventoryRecord>(HttpMethod.Post, "/internal/inventories", request);
    public Task<InventoryRecord> AdjustAsync(long productId, AdjustInventoryRequest request) => SendAsync<InventoryRecord>(HttpMethod.Post, $"/internal/inventories/{productId}/adjustments", request);

    private async Task<T> SendAsync<T>(HttpMethod method, string path, object? body = null)
    {
        using var request = new HttpRequestMessage(method, path);
        if (body is not null) request.Content = JsonContent.Create(body, options: Json);
        using var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode) throw new InventoryGatewayException(response.StatusCode, await response.Content.ReadAsStringAsync());
        return await response.Content.ReadFromJsonAsync<T>(Json) ?? throw new InvalidOperationException("Order engine returned an empty response.");
    }
}

public sealed class InventoryGatewayException(HttpStatusCode statusCode, string detail) : Exception(detail)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}

