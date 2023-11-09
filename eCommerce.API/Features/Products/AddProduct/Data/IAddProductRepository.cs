namespace eCommerce.API.Features.Products.AddProduct.Data;

public interface IAddProductRepository
{
    Task<Guid> AddProductAsync(AddProductCommand request, CancellationToken cancellationToken);
}
