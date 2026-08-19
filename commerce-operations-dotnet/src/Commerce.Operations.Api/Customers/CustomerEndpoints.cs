using FluentValidation;
using MySqlConnector;

namespace Commerce.Operations.Api.Customers;

public static class CustomerEndpoints
{
    public static IEndpointRouteBuilder MapCustomerEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/customers").RequireAuthorization().WithTags("Customers");
        group.MapGet("", async ([AsParameters] CustomerQuery query, IValidator<CustomerQuery> validator, CustomerRepository repository) =>
        {
            var validation = await validator.ValidateAsync(query);
            return validation.IsValid ? Results.Ok(await repository.SearchAsync(query)) : Results.ValidationProblem(validation.ToDictionary());
        });
        group.MapGet("/{id:long}", async (long id, CustomerRepository repository) => await repository.GetAsync(id) is { } customer ? Results.Ok(customer) : Results.NotFound());
        group.MapPost("", async (CreateCustomerRequest request, IValidator<CreateCustomerRequest> validator, CustomerRepository repository) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid) return Results.ValidationProblem(validation.ToDictionary());
            try { var id = await repository.CreateAsync(request); return Results.Created($"/api/customers/{id}", await repository.GetAsync(id)); }
            catch (MySqlException exception) when (exception.Number == 1062) { return Results.Conflict(new { title = "Email already exists" }); }
        }).RequireAuthorization("AdminOnly");
        group.MapPut("/{id:long}", async (long id, UpdateCustomerRequest request, IValidator<UpdateCustomerRequest> validator, CustomerRepository repository) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid) return Results.ValidationProblem(validation.ToDictionary());
            try { return await repository.UpdateAsync(id, request) ? Results.Ok(await repository.GetAsync(id)) : Results.NotFound(); }
            catch (MySqlException exception) when (exception.Number == 1062) { return Results.Conflict(new { title = "Email already exists" }); }
        }).RequireAuthorization("AdminOnly");
        group.MapPatch("/{id:long}/status", async (long id, ChangeCustomerStatusRequest request, IValidator<ChangeCustomerStatusRequest> validator, CustomerRepository repository) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid) return Results.ValidationProblem(validation.ToDictionary());
            return await repository.ChangeStatusAsync(id, request.Status) ? Results.Ok(await repository.GetAsync(id)) : Results.NotFound();
        }).RequireAuthorization("AdminOnly");
        return endpoints;
    }
}
