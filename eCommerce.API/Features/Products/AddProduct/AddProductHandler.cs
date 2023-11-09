using eCommerce.API.Features.Products.AddProduct.Data;
using FluentValidation;
using MediatR;

namespace eCommerce.API.Features.Products.AddProduct;

internal sealed class AddProductHandler : IRequestHandler<AddProductCommand, Guid>
{
    private readonly IValidator<AddProductCommand> _validator;
    private readonly IAddProductRepository _addProductRepository;

    public AddProductHandler(IAddProductRepository addProductRepository, IValidator<AddProductCommand> validator)
    {
        _addProductRepository = addProductRepository;
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
            var insertedId = await _addProductRepository.AddProductAsync(request, cancellationToken);
            return insertedId;
        }
        catch (Exception)
        {
            throw;
        }
    }
}
