using Carter;
using MediatR;
using Npgsql;

namespace eCommerce.API.Features.Products;

public static class DeleteProduct
{
    public sealed class Query : IRequest<Unit>
    {
        public string Id { get; set; } = string.Empty;
    }

    internal sealed class QueryHandler : IRequestHandler<Query, Unit>
    {
        private readonly string _connectionString;

        public QueryHandler(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Database")!;
        }

        public async Task<Unit> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var query = "DELETE FROM Products WHERE id = @Id";
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", request.Id);
                        await command.ExecuteNonQueryAsync();
                    }
                }
                return Unit.Value;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}

public class DeleteProductEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/products/{id}", async (string id, ISender sender) =>
        {
            await sender.Send(new DeleteProduct.Query { Id = id });
            return Results.NoContent();
        });
    }
}
