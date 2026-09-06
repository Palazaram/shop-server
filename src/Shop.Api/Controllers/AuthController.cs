using Microsoft.AspNetCore.Mvc;
using Shop.Api.Authentication;
using Shop.Application.Abstractions;
using Shop.Application.Users.LoginUser;
using Shop.Application.Users.RegisterUser;

namespace Shop.Api.Controllers;

public sealed class AuthController(
    ICommandHandler<RegisterUserCommand, RegisterUserResponse> registerUserHandler,
    ICommandHandler<LoginUserCommand, LoginUserResponse> loginUserHandler,
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
}