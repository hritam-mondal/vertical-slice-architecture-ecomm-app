using Carter;
using eCommerce.API.Contracts;
using Mapster;
using MediatR;

namespace eCommerce.API.Features.Products.AddProduct;

public class AddProductEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/product", async (AddProductRequest request, ISender sender) =>
        {
            var command = request.Adapt<AddProductCommand>();
            var productId = await sender.Send(command);
            return Results.Ok(productId);
        });
    }
}
