using FluentValidation;
namespace Commerce.Operations.Api.Orders;
public sealed class CreateOrderRequestValidator:AbstractValidator<CreateOrderRequest>{public CreateOrderRequestValidator(){RuleFor(x=>x.CustomerId).GreaterThan(0);RuleFor(x=>x.Items).NotEmpty().Must(x=>x.Count<=50);RuleForEach(x=>x.Items).ChildRules(i=>{i.RuleFor(x=>x.ProductId).GreaterThan(0);i.RuleFor(x=>x.Quantity).GreaterThan(0).LessThanOrEqualTo(100000);});}}
public sealed class ShipOrderRequestValidator:AbstractValidator<ShipOrderRequest>{public ShipOrderRequestValidator(){RuleFor(x=>x.Carrier).NotEmpty().MaximumLength(100);RuleFor(x=>x.TrackingNumber).NotEmpty().MaximumLength(100).Matches("^[A-Za-z0-9-]+$");}}
