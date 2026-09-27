using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop.Application.Abstractions;
using Shop.Application.Countries;
using Shop.Application.Countries.ChangeCountrySlug;
using Shop.Application.Countries.CreateCountry;
using Shop.Application.Countries.RenameCountry;
using Shop.Domain.Errors;
using Shop.Domain.Roles;

namespace Shop.Api.Controllers;

[Route("api/countries")]
[Authorize(Roles = RoleNames.Admin)]
public sealed class CountriesController(
    ICommandHandler<CreateCountryCommand, CreateCountryResponse> createHandler,
    ICommandHandler<RenameCountryCommand> renameHandler,
    ICommandHandler<ChangeCountrySlugCommand> changeSlugHandler,
    ICountryQueries countryQueries)
        : ApiControllerBase
{
    [HttpPost]
    [ProducesResponseType<CreateCountryResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        CreateCountryCommand command,
        CancellationToken cancellationToken)
    {
        Result<CreateCountryResponse, Error> result =
            await createHandler.HandleAsync(command, cancellationToken);

        return HandleResult(result, response =>
            StatusCode(StatusCodes.Status201Created, response));
    }

    [HttpPut("{countryId:guid}/name")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Rename(
        Guid countryId,
        RenameCountryRequest request,
        CancellationToken cancellationToken)
    {
        RenameCountryCommand command = new(countryId, request.Name);

        UnitResult<Error> result = await renameHandler.HandleAsync(command, cancellationToken);

        return HandleResult(result);
    }

    [HttpPut("{countryId:guid}/slug")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ChangeSlug(
        Guid countryId,
        ChangeCountrySlugRequest request,
        CancellationToken cancellationToken)
    {
        ChangeCountrySlugCommand command = new(countryId, request.Slug);

        UnitResult<Error> result = await changeSlugHandler.HandleAsync(command, cancellationToken);

        return HandleResult(result);
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType<IReadOnlyList<CountryResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        IReadOnlyList<CountryResponse> countries = await countryQueries.GetAllAsync(cancellationToken);

        return Ok(countries);
    }
}