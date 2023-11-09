using FluentValidation;

namespace eCommerce.API.Features.Products.AddProduct;

public class AddProductValidator : AbstractValidator<AddProductCommand>
{
    public AddProductValidator()
    {
        RuleFor(p => p.Name)
            .NotNull().WithMessage("Product name is required")
            .NotEmpty().WithMessage("Product name should not be empty")
            .MaximumLength(100).WithMessage("Product name cannot be longer than 100 characters");

        RuleFor(p => p.Price)
            .NotNull().WithMessage("Product price is required")
            .GreaterThan(0).WithMessage("Product price must be greater than zero");

        RuleFor(p => p.Description)
            .NotNull().WithMessage("Product description is required");
    }
}
