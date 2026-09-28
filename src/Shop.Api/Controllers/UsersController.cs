using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop.Api.Extensions;
using Shop.Application.Abstractions;
using Shop.Application.Users;
using Shop.Application.Users.ChangePassword;
using Shop.Application.Users.UpdateUserProfile;
using Shop.Domain.Errors;

namespace Shop.Api.Controllers;

[Route("api/users")]
[Authorize]
public sealed class UsersController(
    ICommandHandler<UpdateUserProfileCommand> updateProfileHandler,
    ICommandHandler<ChangePasswordCommand> changePasswordHandler,
    IUserQueries userQueries)
        : ApiControllerBase
{
    /// <summary>
    /// Профиль текущего пользователя. Идентификатор берётся из токена, а не из адреса:
    /// «чужой профиль» — это уже админская задача, и жить она будет отдельно.
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType<UserProfileResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        Maybe<UserProfileResponse> profile =
            await userQueries.GetProfileAsync(User.GetUserId(), cancellationToken);

        // Токен есть, а пользователя нет — учётную запись удалили, пока сессия была жива.
        return profile.HasNoValue
            ? ToActionResult(DomainErrors.Users.NotFound())
            : Ok(profile.Value);
    }

    [HttpPut("me")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateMe(
        UpdateUserProfileRequest request,
        CancellationToken cancellationToken)
    {
        UpdateUserProfileCommand command = new(
            User.GetUserId(),
            request.FirstName,
            request.LastName,
            request.Patronymic,
            request.Phone);

        UnitResult<Error> result = await updateProfileHandler.HandleAsync(command, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Смена пароля гасит все refresh-токены пользователя, включая текущий: после успеха
    /// фронт обязан отправить человека на вход заново.
    /// </summary>
    [HttpPut("me/password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword(
        ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        ChangePasswordCommand command = new(
            User.GetUserId(), request.CurrentPassword, request.NewPassword);

        UnitResult<Error> result =
            await changePasswordHandler.HandleAsync(command, cancellationToken);

        return HandleResult(result);
    }
}
