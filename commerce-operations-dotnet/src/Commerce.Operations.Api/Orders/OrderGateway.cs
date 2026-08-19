using System.Net;using System.Net.Http.Json;using System.Text.Json;
namespace Commerce.Operations.Api.Orders;
public sealed class OrderGateway(HttpClient client){private static readonly JsonSerializerOptions Json=new(JsonSerializerDefaults.Web);
 public Task<OrderPage> ListAsync(int p,int s)=>Send<OrderPage>(HttpMethod.Get,$"/internal/orders?page={p}&pageSize={s}");
 public Task<OrderRecord> GetAsync(long id)=>Send<OrderRecord>(HttpMethod.Get,$"/internal/orders/{id}");
 public Task<OrderRecord> CreateAsync(EngineCreateOrder body)=>Send<OrderRecord>(HttpMethod.Post,"/internal/orders",body);
 public Task<OrderRecord> CancelAsync(long id)=>Send<OrderRecord>(HttpMethod.Post,$"/internal/orders/{id}/cancel");
 public Task<PaymentRecord> PayAsync(long id)=>Send<PaymentRecord>(HttpMethod.Post,$"/internal/orders/{id}/pay");
 public Task<PaymentRecord> RefundAsync(long id)=>Send<PaymentRecord>(HttpMethod.Post,$"/internal/orders/{id}/refund");
 public Task<ShipmentRecord> ShipAsync(long id,ShipOrderRequest request)=>Send<ShipmentRecord>(HttpMethod.Post,$"/internal/orders/{id}/ship",request);
 public Task<ShipmentRecord> DeliverAsync(long id)=>Send<ShipmentRecord>(HttpMethod.Post,$"/internal/orders/{id}/deliver");
 private async Task<T> Send<T>(HttpMethod method,string path,object? body=null){using var req=new HttpRequestMessage(method,path);if(body is not null)req.Content=JsonContent.Create(body,options:Json);using var res=await client.SendAsync(req);if(!res.IsSuccessStatusCode)throw new OrderGatewayException(res.StatusCode,await res.Content.ReadAsStringAsync());return await res.Content.ReadFromJsonAsync<T>(Json)??throw new InvalidOperationException("Order engine returned an empty response.");}}
public sealed class OrderGatewayException(HttpStatusCode status,string detail):Exception(detail){public HttpStatusCode StatusCode{get;}=status;}
