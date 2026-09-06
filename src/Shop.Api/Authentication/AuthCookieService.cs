using Shop.Application.Options;
using Shop.Infrastructure.Options;

namespace Shop.Api.Authentication;

public sealed class AuthCookieService(JwtOptions jwtOptions, RefreshTokenOptions refreshTokenOptions)
{
    public const string AccessTokenCookieName = "access_token";
    public const string RefreshTokenCookieName = "refresh_token";

    private const string AccessTokenPath = "/";
    private const string RefreshTokenPath = "/api/auth";

    public void SetTokens(HttpResponse response, string accessToken, string refreshToken)
    {
        response.Cookies.Append(
            AccessTokenCookieName,
            accessToken,
            BuildOptions(AccessTokenPath, TimeSpan.FromMinutes(jwtOptions.AccessTokenExpirationMinutes)));

        response.Cookies.Append(
            RefreshTokenCookieName,
            refreshToken,
            BuildOptions(RefreshTokenPath, TimeSpan.FromDays(refreshTokenOptions.LifetimeDays)));
    }

    public void ClearTokens(HttpResponse response)
    {
        response.Cookies.Delete(AccessTokenCookieName, BuildOptions(AccessTokenPath));
        response.Cookies.Delete(RefreshTokenCookieName, BuildOptions(RefreshTokenPath));
    }

    private static CookieOptions BuildOptions(string path, TimeSpan? maxAge = null) => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Lax,
        Path = path,
        MaxAge = maxAge
    };
}