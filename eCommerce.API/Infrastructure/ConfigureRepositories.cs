using eCommerce.API.Infrastructure.Data;

namespace eCommerce.API.Infrastructure;

public static partial class ConfigureRepositories
{
    public static void AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IProductsRepository, ProductsRepository>();
    }
}
