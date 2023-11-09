using FluentValidation;
using MediatR;
using Npgsql;
using NpgsqlTypes;

namespace eCommerce.API.Features.Products.AddProduct;


internal sealed class AddProductHandler : IRequestHandler<AddProductCommand, Guid>
{
    private readonly string _connectionString;
    private readonly IValidator<AddProductCommand> _validator;
    public AddProductHandler(IConfiguration configuration, IValidator<AddProductCommand> validator)
    {
        _connectionString = configuration.GetConnectionString("Database")!;
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
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);
                var query = "INSERT INTO Products (id, name, price, description) VALUES (@Id, @Name, @Price, @Description) RETURNING id;";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.Add("@Id", NpgsqlDbType.Uuid).Value = Guid.NewGuid();
                    command.Parameters.Add("@Name", NpgsqlDbType.Varchar).Value = request.Name;
                    command.Parameters.Add("@Price", NpgsqlDbType.Numeric).Value = request.Price;
                    command.Parameters.Add("@Description", NpgsqlDbType.Text).Value = request.Description;

                    var insertedId = await command.ExecuteScalarAsync(cancellationToken);
                    return (Guid)insertedId!;
                }
            }
        }
        catch (Exception)
        {
            throw;
        }
    }
}
