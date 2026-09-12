using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop.Api.Authentication;
using Shop.Api.Contracts;
using Shop.Api.Extensions;
using Shop.Application.Abstractions;
using Shop.Application.RefreshTokens.RefreshAccessToken;
using Shop.Application.RefreshTokens.RevokeRefreshToken;
using Shop.Application.Users.LoginUser;
using Shop.Application.Users.RegisterUser;
using Shop.Domain.Errors;

namespace Shop.Api.Controllers;

[Route("api/auth")]
public sealed class AuthController(
    ICommandHandler<RegisterUserCommand, RegisterUserResponse> registerUserHandler,
    ICommandHandler<LoginUserCommand, LoginUserResponse> loginUserHandler,
    ICommandHandler<RefreshAccessTokenCommand, RefreshAccessTokenResponse> refreshAccessTokenHandler,
    ICommandHandler<RevokeRefreshTokenCommand> revokeRefreshTokenHandler,
    AuthCookieService authCookieService)
        : ApiControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterUserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var result = await registerUserHandler.HandleAsync(command, cancellationToken);

        return HandleResult(result, response 
            => StatusCode(StatusCodes.Status201Created, response));
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginUserCommand command, CancellationToken cancellationToken)
    {
        var result = await loginUserHandler.HandleAsync(command, cancellationToken);

        return HandleResult(result, response =>
        {
            authCookieService.SetTokens(Response, response.AccessToken, response.RefreshToken);
            return NoContent();
        });
    }

    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
    {
        string? refreshToken = Request.Cookies[AuthCookieService.RefreshTokenCookieName];

        if (string.IsNullOrWhiteSpace(refreshToken))
            return ToActionResult(DomainErrors.Auth.RefreshTokenInvalid());

        var result = await refreshAccessTokenHandler.HandleAsync(
            new RefreshAccessTokenCommand(refreshToken), cancellationToken);

        if (result.IsFailure)
        {
            authCookieService.ClearTokens(Response);
            return ToActionResult(result.Error);
        }

        authCookieService.SetTokens(Response, result.Value.AccessToken, result.Value.RefreshToken);

        return NoContent();
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        string? refreshToken = Request.Cookies[AuthCookieService.RefreshTokenCookieName];

        if (!string.IsNullOrWhiteSpace(refreshToken))
            await revokeRefreshTokenHandler.HandleAsync(
                new RevokeRefreshTokenCommand(refreshToken), cancellationToken);

        authCookieService.ClearTokens(Response);

        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(CurrentUserResponse), StatusCodes.Status200OK)]
    public IActionResult Me()
    {
        var response = new CurrentUserResponse(User.GetUserId(), User.GetRole());

        return Ok(response);
    }
}