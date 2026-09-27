using System.Diagnostics;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Shop.Domain.Errors;

namespace Shop.Application.Abstractions;

/// <summary>
/// Пишет в лог исход каждой команды: имя, код отказа и время.
/// Содержимое команды не пишется никогда — там пароли и телефоны.
/// Исключения не ловит: их пишет обработчик на границе API.
/// </summary>
internal sealed class LoggingCommandHandler<TCommand>(
    ICommandHandler<TCommand> inner,
    ILogger<LoggingCommandHandler<TCommand>> logger)
        : ICommandHandler<TCommand>
{
    public async Task<UnitResult<Error>> HandleAsync(
        TCommand command,
        CancellationToken cancellationToken)
    {
        long startedAt = Stopwatch.GetTimestamp();

        UnitResult<Error> result = await inner.HandleAsync(command, cancellationToken);

        double elapsedMs = Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds;

        if (result.IsSuccess)
            logger.LogInformation("{Command} succeeded in {Elapsed:0.0} ms",
                typeof(TCommand).Name, elapsedMs);
        else
            logger.LogWarning("{Command} rejected with {ErrorCode} in {Elapsed:0.0} ms",
                typeof(TCommand).Name, result.Error.Code, elapsedMs);

        return result;
    }
}

internal sealed class LoggingCommandHandler<TCommand, TResponse>(
    ICommandHandler<TCommand, TResponse> inner,
    ILogger<LoggingCommandHandler<TCommand, TResponse>> logger)
        : ICommandHandler<TCommand, TResponse>
{
    public async Task<Result<TResponse, Error>> HandleAsync(
        TCommand command,
        CancellationToken cancellationToken)
    {
        long startedAt = Stopwatch.GetTimestamp();

        Result<TResponse, Error> result = await inner.HandleAsync(command, cancellationToken);

        double elapsedMs = Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds;

        if (result.IsSuccess)
            logger.LogInformation("{Command} succeeded in {Elapsed:0.0} ms",
                typeof(TCommand).Name, elapsedMs);
        else
            logger.LogWarning("{Command} rejected with {ErrorCode} in {Elapsed:0.0} ms",
                typeof(TCommand).Name, result.Error.Code, elapsedMs);

        return result;
    }
}