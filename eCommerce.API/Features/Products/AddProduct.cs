using Carter;
using eCommerce.API.Contracts;
using FluentValidation;
using Mapster;
using MediatR;
using Npgsql;
using NpgsqlTypes;

namespace eCommerce.API.Features.Products;

public class AddProduct
{
    public sealed class Command : IRequest<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(p => p.Name)
                .NotNull().WithMessage("Product name is required")
                .NotEmpty().WithMessage("Product name should not be empty")
                .MaximumLength(100).WithMessage("Product name cannot be longer than 100 characters");

            RuleFor(p => p.Price)
                .NotNull().WithMessage("Product price is required")
                .GreaterThan(0).WithMessage("Product price must be greater than zero");

            RuleFor(p => p.Description)
                .NotNull().WithMessage("Product description is required");
        }
    }

    internal sealed class Handler : IRequestHandler<Command, Guid>
    {
        private readonly string _connectionString;
        private readonly IValidator<Command> _validator;
        public Handler(IConfiguration configuration, IValidator<Command> validator)
        {
            _connectionString = configuration.GetConnectionString("Database")!;
            _validator = validator;
        }

        public async Task<Guid> Handle(Command request, CancellationToken cancellationToken)
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
}

public class AddProductEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/product", async (AddProductRequest request, ISender sender) =>
        {
            var command = request.Adapt<AddProduct.Command>();
            var productId = await sender.Send(command);
            return Results.Ok(productId);
        });
    }
}
