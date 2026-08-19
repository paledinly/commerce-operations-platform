using FluentValidation;

namespace Commerce.Operations.Api.Products;

public sealed class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(50).Matches("^[A-Za-z0-9][A-Za-z0-9._-]*$");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0).LessThanOrEqualTo(999_999_999_999_999.99m);
        RuleFor(x => x.Status).Must(x => ProductStatuses.All.Contains(x.ToUpperInvariant())).WithMessage("Status must be ACTIVE or INACTIVE.");
    }
}

public sealed class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(50).Matches("^[A-Za-z0-9][A-Za-z0-9._-]*$");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0).LessThanOrEqualTo(999_999_999_999_999.99m);
        RuleFor(x => x.Status).Must(x => ProductStatuses.All.Contains(x.ToUpperInvariant())).WithMessage("Status must be ACTIVE or INACTIVE.");
    }
}

public sealed class ChangeProductStatusRequestValidator : AbstractValidator<ChangeProductStatusRequest>
{
    public ChangeProductStatusRequestValidator() => RuleFor(x => x.Status).Must(x => ProductStatuses.All.Contains(x.ToUpperInvariant())).WithMessage("Status must be ACTIVE or INACTIVE.");
}

public sealed class ProductQueryValidator : AbstractValidator<ProductQuery>
{
    private static readonly string[] SortFields = ["sku", "name", "price", "status", "createdAt", "updatedAt"];
    public ProductQueryValidator()
    {
        RuleFor(x => x.Search).MaximumLength(200);
        RuleFor(x => x.Status).Must(x => string.IsNullOrWhiteSpace(x) || ProductStatuses.All.Contains(x.ToUpperInvariant())).WithMessage("Status must be ACTIVE or INACTIVE.");
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.SortBy).Must(x => SortFields.Contains(x)).WithMessage("Unsupported sort field.");
        RuleFor(x => x.SortDirection).Must(x => x.Equals("asc", StringComparison.OrdinalIgnoreCase) || x.Equals("desc", StringComparison.OrdinalIgnoreCase));
    }
}
