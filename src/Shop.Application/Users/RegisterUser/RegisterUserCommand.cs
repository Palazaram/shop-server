namespace Shop.Application.Users.RegisterUser;

public sealed record RegisterUserCommand(
    string FirstName,
    string LastName,
    string Patronymic,
    string Email,
    string Phone,
    string Password);