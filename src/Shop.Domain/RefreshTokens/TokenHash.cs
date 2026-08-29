using CSharpFunctionalExtensions;
using Shop.Domain.Errors;

namespace Shop.Domain.RefreshTokens;

public sealed class TokenHash : SimpleValueObject<string>
{
    private TokenHash(string value) : base(value) { }

    public static Result<TokenHash, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return DomainErrors.RefreshTokens.TokenHashIsRequired();

        return new TokenHash(value);
    }
}