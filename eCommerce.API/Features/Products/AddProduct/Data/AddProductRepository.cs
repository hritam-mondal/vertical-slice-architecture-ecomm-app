using Npgsql;
using NpgsqlTypes;

namespace eCommerce.API.Features.Products.AddProduct.Data;

public class AddProductRepository : IAddProductRepository
{
    private readonly string _connectionString;

    public AddProductRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Database")!;
    }

    public async Task<Guid> AddProductAsync(AddProductCommand request, CancellationToken cancellationToken)
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync(cancellationToken);

            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    var query = "INSERT INTO Products (id, name, price, description) VALUES (@Id, @Name, @Price, @Description) RETURNING id;";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        AddParameter(command, "@Id", NpgsqlDbType.Uuid, Guid.NewGuid());
                        AddParameter(command, "@Name", NpgsqlDbType.Varchar, request.Name);
                        AddParameter(command, "@Price", NpgsqlDbType.Numeric, request.Price);
                        AddParameter(command, "@Description", NpgsqlDbType.Text, request.Description);

                        var insertedId = await command.ExecuteScalarAsync(cancellationToken);
                        transaction.Commit();

                        return (Guid)insertedId!;
                    }
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }

    private static void AddParameter(NpgsqlCommand command, string parameterName, NpgsqlDbType dbType, object value)
    {
        command.Parameters.Add(parameterName, dbType).Value = value;
    }
}
