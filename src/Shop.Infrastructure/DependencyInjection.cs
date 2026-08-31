using Microsoft.Extensions.DependencyInjection;
using Shop.Application.Abstractions;
using Shop.Infrastructure.Security;

namespace Shop.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        return services;
    }
}
