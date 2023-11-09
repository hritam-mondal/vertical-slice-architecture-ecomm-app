using System.Reflection;

namespace eCommerce.API.Infrastructure.IoC;

public static partial class RepositoryRegistration
{
    public static void RegisterRepositories(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        var repositoryInterfaces = assembly.GetTypes()
            .Where(type => type.IsInterface && type.Name.EndsWith("Repository"))
            .ToList();

        foreach (var repositoryInterface in repositoryInterfaces)
        {
            var repositoryImplementation = assembly.GetTypes()
                .FirstOrDefault(type => type.IsClass && repositoryInterface.IsAssignableFrom(type));

            if (repositoryImplementation != null)
            {
                services.AddScoped(repositoryInterface, repositoryImplementation);
            }
        }
    }
}
