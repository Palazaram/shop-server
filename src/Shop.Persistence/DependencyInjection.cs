using Microsoft.Extensions.DependencyInjection;
using Shop.Application.Categories;
using Shop.Application.Manufacturers;
using Shop.Application.ProductAttributes;
using Shop.Application.ProductVariants;
using Shop.Domain.Abstractions;
using Shop.Domain.AttributeValues;
using Shop.Domain.Categories;
using Shop.Domain.Manufacturers;
using Shop.Domain.ProductAttributes;
using Shop.Domain.Products;
using Shop.Domain.ProductVariants;
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
        services.AddScoped<IManufacturerRepository, ManufacturerRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductVariantRepository, ProductVariantRepository>();
        services.AddScoped<IProductAttributeRepository, ProductAttributeRepository>();
        services.AddScoped<IAttributeValueRepository, AttributeValueRepository>();

        services.AddScoped<IManufacturerQueries, ManufacturerQueries>();
        services.AddScoped<ICategoryQueries, CategoryQueries>();
        services.AddScoped<IProductVariantQueries, ProductVariantQueries>();
        services.AddScoped<IProductAttributeQueries, ProductAttributeQueries>();

        return services;
    }
}