using Microsoft.AspNetCore.Mvc;
using Shop.Application.Abstractions;
using Shop.Application.Users.RegisterUser;

namespace Shop.Api.Controllers;

public sealed class UsersController(
    ICommandHandler<RegisterUserCommand, RegisterUserResponse> registerUserHandler)
    : ApiControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterUserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var result = await registerUserHandler.HandleAsync(command, cancellationToken);

        return HandleResult(result, response => Created($"/api/users/{response.UserId}", response));
    }
}
