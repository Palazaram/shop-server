using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Shop.Api.OpenApi;

/// <summary>
/// Приводит required в схеме к тому, что действительно проверяет сервер. Генератор помечает
/// обязательными все параметры конструктора record'а без значения по умолчанию, то есть все поля
/// сразу; здесь набор перезаписывается по валидатору, а у обязательных полей снимается null.
/// </summary>
internal sealed class RequiredFromValidatorSchemaTransformer(
    IServiceScopeFactory scopeFactory,
    ILogger<RequiredFromValidatorSchemaTransformer> logger)
        : IOpenApiSchemaTransformer
{
    private readonly ConcurrentDictionary<Type, HashSet<string>?> requiredByType = new();

    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (schema.Properties is not { Count: > 0 } properties)
            return Task.CompletedTask;

        HashSet<string>? requiredProperties =
            requiredByType.GetOrAdd(context.JsonTypeInfo.Type, InferRequiredProperties);

        // Валидатора нет — значит тип в теле запроса не приходит (ответ, вложенный объект).
        // Там required от генератора верен: сервер эти поля всегда отдаёт.
        if (requiredProperties is null)
            return Task.CompletedTask;

        HashSet<string> required = properties.Keys
            .Where(requiredProperties.Contains)
            .ToHashSet(StringComparer.Ordinal);

        schema.Required = required.Count > 0 ? required : null;

        // Обязательное поле не может быть null — иначе пример в документации предлагает
        // отправить значение, которое валидатор отклонит.
        foreach (string propertyName in required)
            if (properties[propertyName] is OpenApiSchema propertySchema
                && propertySchema.Type is { } propertyType
                && propertyType != JsonSchemaType.Null)
                propertySchema.Type = propertyType & ~JsonSchemaType.Null;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Прогоняет валидатор на пустом экземпляре контракта: поле, на которое пришла ошибка,
    /// обязательно. Возвращает null, если валидатора нет или прогон не удался — тогда схема
    /// остаётся как её собрал генератор.
    /// </summary>
    private HashSet<string>? InferRequiredProperties(Type contractType)
    {
        try
        {
            using IServiceScope scope = scopeFactory.CreateScope();

            if (scope.ServiceProvider.GetService(typeof(IValidator<>).MakeGenericType(contractType))
                    is not IValidator validator)
                return null;

            object emptyContract = RuntimeHelpers.GetUninitializedObject(contractType);

            ValidationResult result =
                validator.Validate(new ValidationContext<object>(emptyContract));

            return result.Errors
                .Select(failure => failure.PropertyName)
                .Where(name => !string.IsNullOrEmpty(name)
                    && !name.Contains('.')
                    && !name.Contains('['))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Could not infer required properties of {Contract} from its validator",
                contractType.Name);

            return null;
        }
    }
}