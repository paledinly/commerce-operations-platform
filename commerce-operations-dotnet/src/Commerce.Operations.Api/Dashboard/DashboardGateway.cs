using System.Net;using System.Net.Http.Json;using System.Text.Json;
namespace Commerce.Operations.Api.Dashboard;
public sealed class DashboardGateway(HttpClient client){private static readonly JsonSerializerOptions Json=new(JsonSerializerDefaults.Web);public async Task<EngineDashboard> GetAsync(){using var response=await client.GetAsync("/internal/dashboard");if(!response.IsSuccessStatusCode)throw new DashboardGatewayException(response.StatusCode,await response.Content.ReadAsStringAsync());return await response.Content.ReadFromJsonAsync<EngineDashboard>(Json)??throw new InvalidOperationException("Order engine returned an empty response.");}}
public sealed class DashboardGatewayException(HttpStatusCode status,string detail):Exception(detail){public HttpStatusCode StatusCode{get;}=status;}
