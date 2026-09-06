using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shop.Application.Abstractions;
using Shop.Application.Options;
using System.Reflection;

namespace Shop.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services, IConfiguration configuration)
    {
        var assembly = typeof(DependencyInjection).Assembly;
        services.AddSingleton(TimeProvider.System);
        services.AddValidatorsFromAssembly(assembly);
        AddHandlers(services, assembly);

        services.AddOptions<RefreshTokenOptions>()
            .Bind(configuration.GetSection(RefreshTokenOptions.SectionName))
            .Validate(o => o.LifetimeDays > 0, "RefreshToken:LifetimeDays must be greater than zero.")
            .ValidateOnStart();

        services.AddSingleton(sp => sp.GetRequiredService<IOptions<RefreshTokenOptions>>().Value);

        return services;
    }

    private static void AddHandlers(IServiceCollection services, Assembly assembly)
    {
        var handlerInterfaces = new[]
        {
            typeof(ICommandHandler<,>),
            typeof(ICommandHandler<>),
            //typeof(IQueryHandler<,>)
        };

        var handlerTypes = assembly.GetTypes()
            .Where(type => type is { IsAbstract: false, IsGenericTypeDefinition: false })
            .Where(type => type.GetInterfaces().Any(i =>
                i.IsGenericType && handlerInterfaces.Contains(i.GetGenericTypeDefinition())));

        foreach (var handlerType in handlerTypes)
        {
            var implementedInterfaces = handlerType.GetInterfaces()
                .Where(i => i.IsGenericType && handlerInterfaces.Contains(i.GetGenericTypeDefinition()));

            foreach (var handlerInterface in implementedInterfaces)
                services.AddScoped(handlerInterface, handlerType);
        }
    }
}