using System.Reflection;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Shop.Application.Abstractions;

namespace Shop.Api.Extensions;

public static class ValidationStartupCheck
{
    /// <summary>
    /// Не даёт приложению подняться, если у типа, который приезжает в теле запроса,
    /// нет валидатора. Исключение оформляется явно — атрибутом [NoValidation].
    /// </summary>
    public static WebApplication EnsureBodyParametersHaveValidators(this WebApplication app)
    {
        var actions = app.Services
            .GetRequiredService<IActionDescriptorCollectionProvider>()
            .ActionDescriptors.Items;

        // Валидаторы зарегистрированы как Scoped, из корневого провайдера их не достать.
        using IServiceScope scope = app.Services.CreateScope();

        List<string> missing = [];

        foreach (ActionDescriptor action in actions)
        {
            if (action is not ControllerActionDescriptor controllerAction)
                continue;

            foreach (ParameterDescriptor parameter in controllerAction.Parameters)
            {
                if (parameter.BindingInfo?.BindingSource != BindingSource.Body)
                    continue;

                Type contract = parameter.ParameterType;

                if (contract.GetCustomAttribute<NoValidationAttribute>() is not null)
                    continue;

                Type validatorType = typeof(IValidator<>).MakeGenericType(contract);

                if (scope.ServiceProvider.GetService(validatorType) is null)
                    missing.Add($"{controllerAction.ControllerName}.{controllerAction.ActionName}: {contract.Name}");
            }
        }

        if (missing.Count > 0)
            throw new InvalidOperationException(
                $"Body parameters without a validator ({missing.Count}): {string.Join("; ", missing)}. " +
                "Add a validator or mark the contract with [NoValidation].");

        return app;
    }
}