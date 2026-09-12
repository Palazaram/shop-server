namespace Shop.Application.Abstractions;

public interface ISlugGenerator
{
    string Generate(string source);
}