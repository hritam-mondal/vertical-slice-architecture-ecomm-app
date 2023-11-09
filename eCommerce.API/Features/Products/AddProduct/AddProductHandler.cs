using eCommerce.API.Infrastructure.Data;
using FluentValidation;
using MediatR;

namespace eCommerce.API.Features.Products.AddProduct;


internal sealed class AddProductHandler : IRequestHandler<AddProductCommand, Guid>
{
    private readonly IValidator<AddProductCommand> _validator;
    private readonly IProductsRepository _productsRepository;
    public AddProductHandler(IProductsRepository productsRepository, IValidator<AddProductCommand> validator)
    {
        _productsRepository = productsRepository;
        _validator = validator;
    }

    public async Task<Guid> Handle(AddProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new InvalidOperationException();
            }
            var insertedId = await _productsRepository.AddProductAsync(request, cancellationToken);
            return insertedId;
        }
        catch (Exception)
        {
            throw;
        }
    }
}
