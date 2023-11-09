using MediatR;

namespace eCommerce.API.Features.Products.AddProduct;

public class AddProductCommand : IRequest<Guid>
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
}
