using eCommerce.API.Features.Products.AddProduct;

namespace eCommerce.API.Infrastructure.Data;

public interface IProductsRepository
{
    Task<Guid> AddProductAsync(AddProductCommand request, CancellationToken cancellationToken);
}
