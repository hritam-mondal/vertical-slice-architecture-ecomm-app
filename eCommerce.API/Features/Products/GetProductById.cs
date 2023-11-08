using Carter;
using eCommerce.API.Entities;
using MediatR;
using Npgsql;

namespace eCommerce.API.Features.Products;

public static class GetProductById
{
    public sealed class Query : IRequest<Product?>
    {
        public Guid Id { get; set; }
    }

    internal sealed class QueryHandler : IRequestHandler<Query, Product?>
    {
        private readonly string _connectionString;

        public QueryHandler(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Database")!;
        }

        public async Task<Product?> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                Product? product = null;
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var query = "SELECT * FROM Products WHERE id = @Id";
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", request.Id);
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                product = new Product
                                {
                                    Id = reader.GetGuid(reader.GetOrdinal("Id")),
                                    Name = reader.GetString(reader.GetOrdinal("Name")),
                                    Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                    Description = reader.GetString(reader.GetOrdinal("Description"))
                                };
                            }
                        }
                    }
                }
                return product;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}

public class GetProductByIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/products/{id}", async (Guid id, ISender sender) =>
        {
            var product = await sender.Send(new GetProductById.Query { Id = id });
            if (product == null)
            {
                return Results.NotFound();
            }
            return Results.Ok(product);
        });
    }
}
