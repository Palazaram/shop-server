using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Shop.Api.Extensions;
using Shop.Domain.Errors;

namespace Shop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>Успех → 200 OK со значением.</summary>
    protected IActionResult HandleResult<T>(Result<T, Error> result)
        => result.IsSuccess
            ? Ok(result.Value)
            : ToActionResult(result.Error);

    /// <summary>Успех → результат, который построит вызывающий (201, 202 и т.д.).</summary>
    protected IActionResult HandleResult<T>(Result<T, Error> result, Func<T, IActionResult> onSuccess)
        => result.IsSuccess
            ? onSuccess(result.Value)
            : ToActionResult(result.Error);

    /// <summary>Успех без данных → 204 No Content.</summary>
    protected IActionResult HandleResult(UnitResult<Error> result)
        => result.IsSuccess
            ? NoContent()
            : ToActionResult(result.Error);

    /// <summary>
    /// 404 с новым адресом в теле (`newSlug`). Редирект посетителю отдаёт фронт: 301 обязан
    /// увидеть поисковик, а он ходит на фронт, не в API. Отдай мы 301 отсюда — `fetch`
    /// проглотил бы его молча, и посетитель остался бы на старом адресе.
    /// </summary>
    protected IActionResult MovedSlug(Error error, string newSlug)
    {
        var problemDetails = error.ToProblemDetails().WithTraceId(HttpContext);
        problemDetails.Extensions["newSlug"] = newSlug;

        return new ObjectResult(problemDetails) { StatusCode = problemDetails.Status };
    }

    protected IActionResult ToActionResult(Error error)
    {
        var problemDetails = error.ToProblemDetails().WithTraceId(HttpContext);

        return new ObjectResult(problemDetails) { StatusCode = problemDetails.Status };
    }
}