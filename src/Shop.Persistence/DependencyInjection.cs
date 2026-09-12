using Microsoft.Extensions.DependencyInjection;
using Shop.Application.Categories;
using Shop.Domain.Abstractions;
using Shop.Domain.Categories;
using Shop.Domain.RefreshTokens;
using Shop.Domain.Users;
using Shop.Persistence.Queries;
using Shop.Persistence.Repositories;

namespace Shop.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(options =>
            DbContextOptionsConfigurator.Configure(options, connectionString));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        services.AddScoped<ICategoryQueries, CategoryQueries>();

        return services;
    }
}