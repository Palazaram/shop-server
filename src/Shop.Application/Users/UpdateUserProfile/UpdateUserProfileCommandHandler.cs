using CSharpFunctionalExtensions;
using Shop.Application.Abstractions;
using Shop.Domain.Abstractions;
using Shop.Domain.Errors;
using Shop.Domain.Users;

namespace Shop.Application.Users.UpdateUserProfile;

internal sealed class UpdateUserProfileCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork)
        : ICommandHandler<UpdateUserProfileCommand>
{
    public async Task<UnitResult<Error>> HandleAsync(
        UpdateUserProfileCommand command,
        CancellationToken cancellationToken)
    {
        Maybe<User> maybeUser = await userRepository.GetByIdAsync(command.UserId, cancellationToken);

        if (maybeUser.HasNoValue)
            return DomainErrors.Users.NotFound();

        User user = maybeUser.Value;

        Result<FullName, Error> fullName = FullName.Create(
            command.FirstName, command.LastName, command.Patronymic);

        if (fullName.IsFailure)
            return fullName.Error;

        Result<Phone, Error> phone = Phone.Create(command.Phone);

        if (phone.IsFailure)
            return phone.Error;

        // Телефон — логин, поэтому уникален. Проверка исключает самого пользователя:
        // «сохранил профиль, не тронув телефон» — не конфликт, а пустая операция.
        if (await userRepository.ExistsByPhoneAsync(phone.Value, user.Id, cancellationToken))
            return DomainErrors.Users.PhoneAlreadyExists();

        // Домен отвечает «не изменилось» на совпадение — для частичного обновления профиля
        // это законный случай, а не ошибка: человек мог поправить только имя.
        user.ChangeFullName(fullName.Value);
        user.ChangePhone(phone.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UnitResult.Success<Error>();
    }
}
