using HrAgencySystem.Identity.Application.Port;
using HrAgencySystem.Identity.Infrastructure.IAM;
using HrAgencySystem.Identity.Infrastructure.Persistence;
using HrAgencySystem.Identity.Infrastructure.Query;
using HrAgencySystem.Identity.Services;
using HrAgencySystem.SharedKernel.Snapshots;
using Microsoft.Extensions.DependencyInjection;

namespace HrAgencySystem.Identity.Infrastructure.Configuration;

internal static class IdentityServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddIdentityServices()
        {
            services.AddTransient<IPasswordHasher, BCryptPasswordHasher>();
            services.AddScoped<IUserEmailReservationRepository, UserEmailReservationRepository>();
            services.AddScoped<IOwnerEmailReservationRepository, OwnerEmailReservationRepository>();
            services.AddScoped<IUserSnapshotRepository, UserSnapshotRepository>();
            services.AddTransient<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IServiceApiKeyRepository, ServiceApiKeyRepository>();
            services.AddOptions<JwtConfig>(JwtConfig.Section);
            services.AddScoped<IUserSuggestionRepository, UserSuggestionRepository>();
            services.AddScoped<IUserQueryRepository, UserQueryRepository>();
            services.AddScoped<IUserProfileRepository, UserProfileRepository>();
            services.AddScoped<IIdentityService, IdentityService>();
        }
    }
}
