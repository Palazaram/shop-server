using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Shop.Application.Abstractions;
using System.Reflection;

namespace Shop.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddSingleton(TimeProvider.System);
        services.AddValidatorsFromAssembly(assembly);
        AddHandlers(services, assembly);

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