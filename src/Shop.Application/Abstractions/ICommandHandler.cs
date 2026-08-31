using CSharpFunctionalExtensions;
using Shop.Domain.Errors;

namespace Shop.Application.Abstractions;

public interface ICommandHandler<in TCommand, TResponse>
{
    Task<Result<TResponse, Error>> HandleAsync(TCommand command, CancellationToken cancellationToken);
}

public interface ICommandHandler<in TCommand>
{
    Task<UnitResult<Error>> HandleAsync(TCommand command, CancellationToken cancellationToken);
}