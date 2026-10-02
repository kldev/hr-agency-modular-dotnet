using System.Text;
using HrAgencySystem.Identity.Infrastructure.IAM;
using Microsoft.Extensions.Options;

namespace HrAgencySystem.Api.Infrastructure;

/// <summary>
/// The rules every service token secret follows, checked at startup: long enough for HMAC-SHA256,
/// and never the key user tokens are signed with - that would make every user token a service token.
/// </summary>
internal static class ServiceSecrets
{
    public static OptionsBuilder<T> ValidateServiceSecret<T>(
        this OptionsBuilder<T> builder,
        string section,
        Func<T, string> secret
    )
        where T : class =>
        builder
            .Validate(
                config => Encoding.UTF8.GetByteCount(secret(config)) >= JwtConfig.MinSecretBytes,
                $"{section}:Secret must be at least {JwtConfig.MinSecretBytes} bytes - it signs "
                    + "HMAC-SHA256 service tokens."
            )
            .Validate<IOptions<JwtConfig>>(
                (config, jwt) => secret(config) != jwt.Value.SecretKey,
                $"{section}:Secret must differ from {JwtConfig.Section}:SecretKey."
            );

    public static OptionsBuilder<T> ValidateBaseUrl<T>(
        this OptionsBuilder<T> builder,
        string section,
        Func<T, string> baseUrl
    )
        where T : class =>
        builder.Validate(
            config => IsHttpUrl(baseUrl(config)),
            $"{section}:BaseUrl must be an absolute http(s) url."
        );

    /// <summary>
    /// Absolute is not enough: on Linux "/portal" parses as an absolute file:// uri.
    /// </summary>
    public static bool IsHttpUrl(string value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}
