using Commerce.Operations.Api.Products;
using FluentValidation;

namespace Commerce.Operations.Api.Inventories;

public static class InventoryEndpoints
{
    public static IEndpointRouteBuilder MapInventoryEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/inventories").RequireAuthorization().WithTags("Inventories");
        group.MapGet("", async (int page, int pageSize, InventoryGateway gateway, ProductRepository products) =>
        {
            if (page < 1 || pageSize is < 1 or > 100) return Results.ValidationProblem(new Dictionary<string, string[]> { ["pagination"] = ["Page must be >= 1 and pageSize must be between 1 and 100."] });
            try
            {
                var source = await gateway.ListAsync(page, pageSize);
                var items = await Task.WhenAll(source.Items.Select(async inventory =>
                {
                    var product = await products.GetAsync(inventory.ProductId);
                    return new InventoryView(inventory.ProductId, product?.Sku, product?.Name, inventory.AvailableQuantity, inventory.ReservedQuantity, inventory.Version, inventory.UpdatedAt);
                }));
                return Results.Ok(new InventoryViewPage(items, source.Page, source.PageSize, source.TotalCount));
            }
            catch (InventoryGatewayException exception) { return GatewayError(exception); }
        });
        group.MapGet("/{productId:long}/movements", async (long productId, InventoryGateway gateway) =>
        {
            try { return Results.Ok(await gateway.MovementsAsync(productId)); }
            catch (InventoryGatewayException exception) { return GatewayError(exception); }
        });
        group.MapPost("", async (CreateInventoryRequest request, IValidator<CreateInventoryRequest> validator, ProductRepository products, InventoryGateway gateway) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid) return Results.ValidationProblem(validation.ToDictionary());
            if (await products.GetAsync(request.ProductId) is null) return Results.NotFound(new { title = "Product not found" });
            try { return Results.Created($"/api/inventories/{request.ProductId}", await gateway.CreateAsync(request)); }
            catch (InventoryGatewayException exception) { return GatewayError(exception); }
        }).RequireAuthorization("AdminOnly");
        group.MapPost("/{productId:long}/adjustments", async (long productId, AdjustInventoryRequest request, IValidator<AdjustInventoryRequest> validator, InventoryGateway gateway) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid) return Results.ValidationProblem(validation.ToDictionary());
            try { return Results.Ok(await gateway.AdjustAsync(productId, request)); }
            catch (InventoryGatewayException exception) { return GatewayError(exception); }
        }).RequireAuthorization("AdminOnly");
        return endpoints;
    }

    private static IResult GatewayError(InventoryGatewayException exception) => Results.Problem(statusCode: (int)exception.StatusCode, title: "Order engine request failed", detail: exception.Message.Length > 500 ? exception.Message[..500] : exception.Message);
}
