using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;

namespace HrAgencySystem.Web.Services;

public static class JobBoardClientRegistration
{
    public const string MissingConfigMessage =
        "InternalApi:BaseUrl and InternalApi:ApiKey must be set. Issue a key as the platform owner "
        + "(POST /api/owners/api-keys) and put its sk_ value in user secrets or the environment.";

    extension(IServiceCollection services)
    {
        /// <summary>
        /// The client, with timeouts and retries - but retries on reads only. An application that
        /// timed out may still have been created, and sending it again would file it twice.
        /// </summary>
        public void AddJobBoardClient(IConfiguration configuration)
        {
            services
                .AddOptions<InternalApiConfig>()
                .Bind(configuration.GetSection(InternalApiConfig.Section))
                .Validate(
                    config =>
                        Uri.TryCreate(config.BaseUrl, UriKind.Absolute, out _)
                        && config.ApiKey.StartsWith("sk_", StringComparison.Ordinal),
                    MissingConfigMessage
                )
                .ValidateOnStart();

            services
                .AddHttpClient<IJobBoardClient, HttpJobBoardClient>(
                    (provider, client) =>
                    {
                        var config = provider.GetRequiredService<IOptions<InternalApiConfig>>().Value;

                        client.BaseAddress = new Uri(config.BaseUrl.TrimEnd('/') + "/");
                        client.DefaultRequestHeaders.Add(HttpJobBoardClient.ApiKeyHeader, config.ApiKey);
                    }
                )
                .AddStandardResilienceHandler(options =>
                {
                    var seconds = configuration.GetValue(
                        $"{InternalApiConfig.Section}:{nameof(InternalApiConfig.TimeoutSeconds)}",
                        10
                    );

                    options.Retry.DisableForUnsafeHttpMethods();
                    options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(Math.Max(1, seconds / 3));
                    options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(seconds);
                    // The circuit breaker's window has to cover at least two attempts.
                    options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(
                        Math.Max(30, seconds * 2)
                    );
                });

            services.AddHttpClient(ApiHealthClient);
            services.AddSingleton<ApiHealthProbe>();
        }
    }

    /// <summary>A plain client for <c>/healthz</c>, which asks the API without a key.</summary>
    public const string ApiHealthClient = "api-health";
}
