using FluentValidation;

namespace Commerce.Operations.Api.Inventories;

public sealed class CreateInventoryRequestValidator : AbstractValidator<CreateInventoryRequest>
{
    public CreateInventoryRequestValidator() { RuleFor(x => x.ProductId).GreaterThan(0); RuleFor(x => x.InitialQuantity).GreaterThanOrEqualTo(0); }
}
public sealed class AdjustInventoryRequestValidator : AbstractValidator<AdjustInventoryRequest>
{
    public AdjustInventoryRequestValidator() { RuleFor(x => x.QuantityDelta).NotEqual(0); RuleFor(x => x.Reason).NotEmpty().MaximumLength(200); }
}

