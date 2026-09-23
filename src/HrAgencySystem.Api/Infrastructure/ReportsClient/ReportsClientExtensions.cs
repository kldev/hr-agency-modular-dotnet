using HrAgencySystem.ReportsService.Contracts;

namespace HrAgencySystem.Api.Infrastructure.ReportsClient;

public static class ReportsClientExtensions
{
    extension(IServiceCollection services)
    {
        public void AddReportsClient(IConfiguration configuration)
        {
            var section = configuration.GetSection(ReportsClientConfig.SectionName);
            services.Configure<ReportsClientConfig>(section);

            var config = section.Get<ReportsClientConfig>() ?? new ReportsClientConfig();

            services.AddSingleton<ReportsTokenFactory>();
            services.AddSingleton<ReportsHealthProbe>();
            services.AddHttpClient<IReportsClient, HttpReportsClient>(client =>
            {
                client.BaseAddress = new Uri(
                    string.IsNullOrWhiteSpace(config.BaseUrl)
                        ? "http://localhost:5200"
                        : config.BaseUrl
                );
                client.Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds);
            });
        }
    }
}
