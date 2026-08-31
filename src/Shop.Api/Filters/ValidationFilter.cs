using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Shop.Api.Extensions;

namespace Shop.Api.Filters;

public sealed class ValidationFilter : IAsyncActionFilter
{
    private readonly IServiceProvider _serviceProvider;

    public ValidationFilter(IServiceProvider serviceProvider)
        => _serviceProvider = serviceProvider;

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
                continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());

            if (_serviceProvider.GetService(validatorType) is not IValidator validator)
                continue;

            var validationContext = new ValidationContext<object>(argument);
            var validationResult = await validator.ValidateAsync(
                validationContext, context.HttpContext.RequestAborted);

            if (validationResult.IsValid)
                continue;

            var problemDetails = validationResult.ToProblemDetails();

            context.Result = new ObjectResult(problemDetails)
            {
                StatusCode = problemDetails.Status
            };

            return;
        }

        await next();
    }
}
