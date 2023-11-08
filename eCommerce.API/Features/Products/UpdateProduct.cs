using Carter;
using eCommerce.API.Contracts;
using FluentValidation;
using Mapster;
using MediatR;
using Npgsql;
using NpgsqlTypes;

namespace eCommerce.API.Features.Products;

public static class UpdateProduct
{
    public sealed class Command : IRequest<Unit>
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(p => p.Id)
            .NotNull().WithMessage("Product ID is required")
            .Must(guid => Guid.TryParse(guid.ToString(), out _)).WithMessage("Invalid Product ID");

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

    internal sealed class Handler : IRequestHandler<Command, Unit>
    {
        private readonly string _connectionString;
        private readonly IValidator<Command> _validator;

        public Handler(IConfiguration configuration, IValidator<Command> validator)
        {
            _connectionString = configuration.GetConnectionString("Database")!;
            _validator = validator;
        }

        public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
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

                    using (var command = new NpgsqlCommand("UPDATE Products SET name = @Name, price = @Price, description = @Description WHERE id = @Id", connection))
                    {
                        command.Parameters.Add("@Id", NpgsqlDbType.Uuid).Value = request.Id;
                        command.Parameters.Add("@Name", NpgsqlDbType.Varchar).Value = request.Name;
                        command.Parameters.Add("@Price", NpgsqlDbType.Numeric).Value = request.Price;
                        command.Parameters.Add("@Description", NpgsqlDbType.Text).Value = request.Description;

                        await command.ExecuteNonQueryAsync(cancellationToken);
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

public class UpdateProductEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("api/product/{id}", async (UpdateProductRequest request, ISender sender, Guid id) =>
        {
            var command = request.Adapt<UpdateProduct.Command>();
            command.Id = id;
            await sender.Send(command);
            return Results.Ok();
        });
    }
}