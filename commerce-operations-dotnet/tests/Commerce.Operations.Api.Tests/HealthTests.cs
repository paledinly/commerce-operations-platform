using Commerce.Operations.Api.Auth;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Commerce.Operations.Api.Tests;

public sealed class TestApplicationFactory : WebApplicationFactory<Program>
{
    public TestApplicationFactory() =>
        Environment.SetEnvironmentVariable("Jwt__Secret", "test-only-jwt-secret-at-least-32-bytes-long");

    protected override void ConfigureWebHost(IWebHostBuilder builder) => builder.ConfigureAppConfiguration((_, config) =>
        config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Secret"] = "test-only-jwt-secret-at-least-32-bytes-long",
            ["Jwt:Issuer"] = "test-issuer",
            ["Jwt:Audience"] = "test-audience"
        }));
}

public sealed class HealthTests : IClassFixture<TestApplicationFactory>
{
    private readonly HttpClient client;
    public HealthTests(TestApplicationFactory factory) => client = factory.CreateClient();

    [Fact]
    public async Task Health_is_ok() => Assert.True((await client.GetAsync("/health")).IsSuccessStatusCode);

    [Fact]
    public async Task Correlation_id_and_security_headers_are_returned()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add("X-Correlation-ID", "phase12-test");
        using var response = await client.SendAsync(request);
        Assert.Equal("phase12-test", response.Headers.GetValues("X-Correlation-ID").Single());
        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.Equal("DENY", response.Headers.GetValues("X-Frame-Options").Single());
    }

    [Fact]
    public async Task Me_requires_authentication() => Assert.Equal(System.Net.HttpStatusCode.Unauthorized, (await client.GetAsync("/api/auth/me")).StatusCode);

    [Fact]
    public async Task Products_require_authentication() => Assert.Equal(System.Net.HttpStatusCode.Unauthorized, (await client.GetAsync("/api/products")).StatusCode);

    [Fact]
    public async Task Customers_require_authentication() => Assert.Equal(System.Net.HttpStatusCode.Unauthorized, (await client.GetAsync("/api/customers")).StatusCode);

    [Fact]
    public async Task Inventories_require_authentication() => Assert.Equal(System.Net.HttpStatusCode.Unauthorized, (await client.GetAsync("/api/inventories?page=1&pageSize=20")).StatusCode);

    [Fact]
    public async Task Orders_require_authentication() => Assert.Equal(System.Net.HttpStatusCode.Unauthorized, (await client.GetAsync("/api/orders?page=1&pageSize=20")).StatusCode);

    [Fact]
    public async Task Dashboard_requires_authentication() => Assert.Equal(System.Net.HttpStatusCode.Unauthorized, (await client.GetAsync("/api/dashboard")).StatusCode);

    [Fact]
    public async Task Settlements_require_authentication() => Assert.Equal(System.Net.HttpStatusCode.Unauthorized, (await client.GetAsync("/api/settlements?from=2026-01-01&to=2026-01-31")).StatusCode);

    [Fact]
    public async Task Audit_logs_require_authentication() => Assert.Equal(System.Net.HttpStatusCode.Unauthorized, (await client.GetAsync("/api/audit-logs?page=1&pageSize=20")).StatusCode);
}

public sealed class PasswordServiceTests
{
    [Fact]
    public void Hash_round_trip_is_valid()
    {
        var service = new PasswordService();
        var hash = service.Hash("correct horse battery staple");
        Assert.True(service.Verify("correct horse battery staple", hash));
        Assert.False(service.Verify("wrong password", hash));
        Assert.DoesNotContain("correct horse", hash);
    }
}
