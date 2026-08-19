using FluentValidation;

namespace Commerce.Operations.Api.Customers;

public static class CustomerValidationRules
{
    public static void AddCustomerRules<T>(this AbstractValidator<T> validator, Func<T, string> email, Func<T, string> name, Func<T, string> phone, Func<T, string> status)
    {
        validator.RuleFor(x => email(x)).NotEmpty().EmailAddress().MaximumLength(255).WithName("Email");
        validator.RuleFor(x => name(x)).NotEmpty().MaximumLength(100).WithName("Name");
        validator.RuleFor(x => phone(x)).NotEmpty().Must(value => System.Text.RegularExpressions.Regex.IsMatch(CustomerNormalization.Phone(value), "^\\+?[0-9]{8,15}$")).WithMessage("Phone must contain 8 to 15 digits with an optional leading +.").WithName("Phone");
        validator.RuleFor(x => status(x)).Must(value => CustomerStatuses.All.Contains(value.ToUpperInvariant())).WithMessage("Status must be ACTIVE, SUSPENDED or WITHDRAWN.").WithName("Status");
    }
}

public sealed class CreateCustomerRequestValidator : AbstractValidator<CreateCustomerRequest>
{
    public CreateCustomerRequestValidator() => this.AddCustomerRules(x => x.Email, x => x.Name, x => x.Phone, x => x.Status);
}
public sealed class UpdateCustomerRequestValidator : AbstractValidator<UpdateCustomerRequest>
{
    public UpdateCustomerRequestValidator() => this.AddCustomerRules(x => x.Email, x => x.Name, x => x.Phone, x => x.Status);
}
public sealed class ChangeCustomerStatusRequestValidator : AbstractValidator<ChangeCustomerStatusRequest>
{
    public ChangeCustomerStatusRequestValidator() => RuleFor(x => x.Status).Must(value => CustomerStatuses.All.Contains(value.ToUpperInvariant())).WithMessage("Status must be ACTIVE, SUSPENDED or WITHDRAWN.");
}
public sealed class CustomerQueryValidator : AbstractValidator<CustomerQuery>
{
    private static readonly string[] SortFields = ["email", "name", "status", "createdAt", "updatedAt"];
    public CustomerQueryValidator()
    {
        RuleFor(x => x.Search).MaximumLength(255);
        RuleFor(x => x.Status).Must(value => string.IsNullOrWhiteSpace(value) || CustomerStatuses.All.Contains(value.ToUpperInvariant())).WithMessage("Unsupported status.");
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.SortBy).Must(SortFields.Contains).WithMessage("Unsupported sort field.");
        RuleFor(x => x.SortDirection).Must(value => value.Equals("asc", StringComparison.OrdinalIgnoreCase) || value.Equals("desc", StringComparison.OrdinalIgnoreCase));
    }
}

