using Commerce.Operations.Api.Orders;
using Xunit;
namespace Commerce.Operations.Api.Tests;
public sealed class OrderValidationTests {
 [Fact] public void Valid_order_is_accepted()=>Assert.True(new CreateOrderRequestValidator().Validate(new CreateOrderRequest(1,[new(2,3)])).IsValid);
 [Theory][InlineData(0,1,1)][InlineData(1,0,1)][InlineData(1,1,0)] public void Invalid_order_is_rejected(long customerId,long productId,long quantity)=>Assert.False(new CreateOrderRequestValidator().Validate(new CreateOrderRequest(customerId,[new(productId,quantity)])).IsValid);
 [Fact] public void Valid_shipment_is_accepted()=>Assert.True(new ShipOrderRequestValidator().Validate(new ShipOrderRequest("LOCAL","TRACK-123")).IsValid);
 [Theory][InlineData("","TRACK-1")][InlineData("LOCAL","")][InlineData("LOCAL","TRACK 1")] public void Invalid_shipment_is_rejected(string carrier,string tracking)=>Assert.False(new ShipOrderRequestValidator().Validate(new ShipOrderRequest(carrier,tracking)).IsValid);
}
