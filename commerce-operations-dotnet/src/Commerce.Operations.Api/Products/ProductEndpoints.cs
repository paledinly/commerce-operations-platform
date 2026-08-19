using FluentValidation;
using MySqlConnector;

namespace Commerce.Operations.Api.Products;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/products").RequireAuthorization().WithTags("Products");

        group.MapGet("", async ([AsParameters] ProductQuery query, IValidator<ProductQuery> validator, ProductRepository repository) =>
        {
            var validation = await validator.ValidateAsync(query);
            return validation.IsValid ? Results.Ok(await repository.SearchAsync(query)) : Results.ValidationProblem(validation.ToDictionary());
        });
        group.MapGet("/{id:long}", async (long id, ProductRepository repository) =>
            await repository.GetAsync(id) is { } product ? Results.Ok(product) : Results.NotFound());

        group.MapPost("", async (CreateProductRequest request, IValidator<CreateProductRequest> validator, ProductRepository repository) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid) return Results.ValidationProblem(validation.ToDictionary());
            try
            {
                var id = await repository.CreateAsync(request);
                return Results.Created($"/api/products/{id}", await repository.GetAsync(id));
            }
            catch (MySqlException exception) when (exception.Number == 1062) { return Results.Conflict(new { title = "SKU already exists" }); }
        }).RequireAuthorization("AdminOnly");

        group.MapPut("/{id:long}", async (long id, UpdateProductRequest request, IValidator<UpdateProductRequest> validator, ProductRepository repository) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid) return Results.ValidationProblem(validation.ToDictionary());
            try { return await repository.UpdateAsync(id, request) ? Results.Ok(await repository.GetAsync(id)) : Results.NotFound(); }
            catch (MySqlException exception) when (exception.Number == 1062) { return Results.Conflict(new { title = "SKU already exists" }); }
        }).RequireAuthorization("AdminOnly");

        group.MapPatch("/{id:long}/status", async (long id, ChangeProductStatusRequest request, IValidator<ChangeProductStatusRequest> validator, ProductRepository repository) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid) return Results.ValidationProblem(validation.ToDictionary());
            return await repository.ChangeStatusAsync(id, request.Status) ? Results.Ok(await repository.GetAsync(id)) : Results.NotFound();
        }).RequireAuthorization("AdminOnly");
        return endpoints;
    }
}
