using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System.Reflection;

namespace Shop.Api.OpenApi;

/// <summary>
/// Генератор описывает форму с файлом как application/x-www-form-urlencoded, а ApiExplorer
/// к этому моменту уже разложил IFormFile на его собственные свойства (ContentType, Length,
/// FileName…). Поэтому тело формы собираем из параметров самого метода: там файл ещё цел.
/// </summary>
internal sealed class MultipartFormOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (context.Description.ActionDescriptor is not ControllerActionDescriptor action)
            return Task.CompletedTask;

        ParameterInfo[] formParameters = [.. action.MethodInfo.GetParameters()
            .Where(parameter => parameter.GetCustomAttribute<FromFormAttribute>() is not null
                || IsFile(parameter.ParameterType))];

        if (formParameters.Length == 0)
            return Task.CompletedTask;

        var properties = new Dictionary<string, IOpenApiSchema>(StringComparer.Ordinal);
        var required = new HashSet<string>(StringComparer.Ordinal);

        foreach (ParameterInfo parameter in formParameters)
        {
            string name = parameter.GetCustomAttribute<FromFormAttribute>()?.Name ?? parameter.Name!;

            properties[name] = Describe(parameter.ParameterType);

            // Файл в сигнатуре объявлен как IFormFile? только чтобы отдать свою ошибку
            // вместо сухого 400 от биндера. Для клиента он обязателен.
            if (IsFile(parameter.ParameterType))
                required.Add(name);
        }

        var schema = new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            Properties = properties
        };

        if (required.Count > 0)
            schema.Required = required;

        operation.RequestBody = new OpenApiRequestBody
        {
            Required = true,
            Content = new Dictionary<string, OpenApiMediaType>(StringComparer.Ordinal)
            {
                ["multipart/form-data"] = new OpenApiMediaType { Schema = schema }
            }
        };

        return Task.CompletedTask;
    }

    private static bool IsFile(Type type)
        => type == typeof(IFormFile) || type == typeof(IFormFileCollection);

    private static OpenApiSchema Describe(Type type)
    {
        if (IsFile(type))
            return new OpenApiSchema { Type = JsonSchemaType.String, Format = "binary" };

        Type underlying = Nullable.GetUnderlyingType(type) ?? type;

        if (underlying == typeof(int))
            return new OpenApiSchema { Type = JsonSchemaType.Integer, Format = "int32" };

        if (underlying == typeof(bool))
            return new OpenApiSchema { Type = JsonSchemaType.Boolean };

        if (underlying == typeof(Guid))
            return new OpenApiSchema { Type = JsonSchemaType.String, Format = "uuid" };

        return new OpenApiSchema { Type = JsonSchemaType.String };
    }
}