using Carter;
using eCommerce.API.Entities;
using MediatR;
using Npgsql;

namespace eCommerce.API.Features.Products;

public static class GetAllProducts
{
    public sealed class Query : IRequest<List<Product>>
    {
    }

    internal sealed class QueryHandler : IRequestHandler<Query, List<Product>>
    {
        private readonly string _connectionString;

        public QueryHandler(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Database")!;
        }

        public async Task<List<Product>> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                var products = new List<Product>();
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var query = "SELECT * FROM Products";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var product = new Product
                                {
                                    Id = reader.GetGuid(reader.GetOrdinal("Id")),
                                    Name = reader.GetString(reader.GetOrdinal("Name")),
                                    Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                    Description = reader.GetString(reader.GetOrdinal("Description"))
                                };
                                products.Add(product);
                            }
                        }
                    }
                }
                return products;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}

public class GetAllProductsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/products", async (ISender sender) =>
        {
            var products = await sender.Send(new GetAllProducts.Query());
            return Results.Ok(products);
        });
    }
}
