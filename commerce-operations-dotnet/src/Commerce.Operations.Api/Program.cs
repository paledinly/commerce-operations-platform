using System.Text;
using Commerce.Operations.Api.Auth;
using Commerce.Operations.Api.Customers;
using Commerce.Operations.Api.Inventories;
using Commerce.Operations.Api.Orders;
using Commerce.Operations.Api.Dashboard;
using Commerce.Operations.Api.Settlements;
using Commerce.Operations.Api.Audit;
using Commerce.Operations.Api.Operations;
using Commerce.Operations.Api.Products;
using Dapper;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using MySqlConnector;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()
    ?? throw new InvalidOperationException("JWT configuration is required.");
if (Encoding.UTF8.GetByteCount(jwt.Secret) < 32)
    throw new InvalidOperationException("Jwt:Secret must contain at least 32 UTF-8 bytes.");

builder.Services.AddSingleton(jwt);
builder.Services.AddSingleton<PasswordService>();
builder.Services.AddScoped<OperatorRepository>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<ProductRepository>();
builder.Services.AddScoped<CustomerRepository>();
builder.Services.AddScoped<DashboardRepository>();
builder.Services.AddScoped<AuditRepository>();
builder.Services.AddScoped<AuditMiddleware>();
builder.Services.AddScoped<CorrelationMiddleware>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<SafeGetRetryHandler>();
builder.Services.AddHttpClient<InventoryGateway>((services, client) =>
{
    var configuration = services.GetRequiredService<IConfiguration>();
    client.BaseAddress = new Uri(configuration["OrderEngine:BaseUrl"] ?? "http://localhost:8080");
    client.DefaultRequestHeaders.Add("X-Internal-Api-Key", configuration["OrderEngine:InternalApiKey"] ?? string.Empty);
    client.Timeout = TimeSpan.FromSeconds(10);
}).AddHttpMessageHandler<SafeGetRetryHandler>();
builder.Services.AddHttpClient<OrderGateway>((services, client) =>
{
    var configuration = services.GetRequiredService<IConfiguration>();
    client.BaseAddress = new Uri(configuration["OrderEngine:BaseUrl"] ?? "http://localhost:8080");
    client.DefaultRequestHeaders.Add("X-Internal-Api-Key", configuration["OrderEngine:InternalApiKey"] ?? string.Empty);
    client.Timeout = TimeSpan.FromSeconds(15);
}).AddHttpMessageHandler<SafeGetRetryHandler>();
builder.Services.AddHttpClient<DashboardGateway>((services, client) =>
{
    var configuration = services.GetRequiredService<IConfiguration>();
    client.BaseAddress = new Uri(configuration["OrderEngine:BaseUrl"] ?? "http://localhost:8080");
    client.DefaultRequestHeaders.Add("X-Internal-Api-Key", configuration["OrderEngine:InternalApiKey"] ?? string.Empty);
    client.Timeout = TimeSpan.FromSeconds(10);
}).AddHttpMessageHandler<SafeGetRetryHandler>();
builder.Services.AddHttpClient<SettlementGateway>((services, client) =>
{
    var configuration = services.GetRequiredService<IConfiguration>();
    client.BaseAddress = new Uri(configuration["OrderEngine:BaseUrl"] ?? "http://localhost:8080");
    client.DefaultRequestHeaders.Add("X-Internal-Api-Key", configuration["OrderEngine:InternalApiKey"] ?? string.Empty);
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<SafeGetRetryHandler>();
builder.Services.AddHttpClient("order-engine-health", (services, client) =>
{
    var configuration = services.GetRequiredService<IConfiguration>();
    client.BaseAddress = new Uri(configuration["OrderEngine:BaseUrl"] ?? "http://localhost:8080");
    client.Timeout = TimeSpan.FromSeconds(3);
});
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwt.Issuer,
        ValidAudience = jwt.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret)),
        ClockSkew = TimeSpan.FromSeconds(30)
    });
builder.Services.AddAuthorization(options => options.AddPolicy("AdminOnly", policy => policy.RequireRole("ADMIN")));
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("login", limiter => { limiter.PermitLimit = 10; limiter.Window = TimeSpan.FromMinutes(1); limiter.QueueLimit = 0; });
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }] = Array.Empty<string>()
    });
});
builder.Services.AddHealthChecks().AddCheck<DependencyHealthCheck>("dependencies", tags: ["ready"]);

var app = builder.Build();
app.UseSerilogRequestLogging();
app.UseMiddleware<CorrelationMiddleware>();
app.Use(async (context, next) => { context.Response.Headers["X-Content-Type-Options"] = "nosniff"; context.Response.Headers["X-Frame-Options"] = "DENY"; context.Response.Headers["Referrer-Policy"] = "no-referrer"; await next(); });
app.UseSwagger();
app.UseSwaggerUI();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<AuditMiddleware>();
app.MapHealthChecks("/health", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") });

var connectionString = builder.Configuration.GetConnectionString("Operations");
if (!string.IsNullOrWhiteSpace(connectionString))
{
    await DatabaseBootstrap.MigrateAsync(connectionString, app.Environment.ContentRootPath);
    await DatabaseBootstrap.SeedOperatorAsync(connectionString, builder.Configuration, app.Services.GetRequiredService<PasswordService>());
}

app.MapPost("/api/auth/login", async (LoginRequest request, IValidator<LoginRequest> validator, OperatorRepository repository, PasswordService passwords, TokenService tokens) =>
{
    var validation = await validator.ValidateAsync(request);
    if (!validation.IsValid)
        return Results.ValidationProblem(validation.ToDictionary());

    var account = await repository.FindByEmailAsync(request.Email);
    if (account is null || !account.IsActive || !passwords.Verify(request.Password, account.PasswordHash))
        return Results.Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Invalid credentials");

    return Results.Ok(tokens.Create(account));
}).AllowAnonymous().RequireRateLimiting("login").WithTags("Authentication");

app.MapGet("/api/auth/me", (System.Security.Claims.ClaimsPrincipal user) => Results.Ok(new
{
    id = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
    email = user.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value,
    displayName = user.Identity?.Name,
    role = user.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
})).RequireAuthorization().WithTags("Authentication");

app.MapProductEndpoints();
app.MapCustomerEndpoints();
app.MapInventoryEndpoints();
app.MapOrderEndpoints();
app.MapDashboardEndpoints();
app.MapSettlementEndpoints();
app.MapAuditEndpoints();

app.Run();
public partial class Program { }
