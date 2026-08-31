using CSharpFunctionalExtensions;
using Shop.Application.Abstractions;
using Shop.Domain.Abstractions;
using Shop.Domain.Errors;
using Shop.Domain.Roles;
using Shop.Domain.Users;

namespace Shop.Application.Users.RegisterUser;

public sealed class RegisterUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
        : ICommandHandler<RegisterUserCommand, RegisterUserResponse>
{
    public async Task<Result<RegisterUserResponse, Error>> HandleAsync(
        RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var fullNameResult = FullName.Create(command.FirstName, command.LastName, command.Patronymic);
        if (fullNameResult.IsFailure)
            return fullNameResult.Error;

        var emailResult = Email.Create(command.Email);
        if (emailResult.IsFailure)
            return emailResult.Error;

        var phoneResult = Phone.Create(command.Phone);
        if (phoneResult.IsFailure)
            return phoneResult.Error;

        var existsByEmail = await userRepository.ExistsByEmailAsync(emailResult.Value, cancellationToken);
        if (existsByEmail)
            return DomainErrors.Users.EmailAlreadyExists();

        var existsByPhone = await userRepository.ExistsByPhoneAsync(phoneResult.Value, cancellationToken);
        if (existsByPhone)
            return DomainErrors.Users.PhoneAlreadyExists();

        var passwordHash = passwordHasher.Hash(command.Password);

        var passwordHashResult = PasswordHash.Create(passwordHash);
        if (passwordHashResult.IsFailure)
            return passwordHashResult.Error;

        var user = User.Create(
            fullNameResult.Value,
            phoneResult.Value,
            passwordHashResult.Value,
            emailResult.Value,
            RoleIds.Customer,
            timeProvider.GetUtcNow());

        userRepository.Add(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new RegisterUserResponse(user.Id);
    }
}