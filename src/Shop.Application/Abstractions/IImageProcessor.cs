using CSharpFunctionalExtensions;
using Shop.Domain.Errors;

namespace Shop.Application.Abstractions;

public interface IImageProcessor
{
    Result<IReadOnlyList<ImageVariant>, Error> CreateVariants(Stream source);
}

public sealed record ImageVariant(string Size, byte[] Content);