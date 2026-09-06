namespace Shop.Application.Options;

public sealed class RefreshTokenOptions
{
    public const string SectionName = "RefreshToken";

    public int LifetimeDays { get; init; }
    public int RetentionDaysAfterExpiry { get; init; }
}