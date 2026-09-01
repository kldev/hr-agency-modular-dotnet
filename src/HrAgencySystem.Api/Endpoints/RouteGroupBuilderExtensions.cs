using HrAgencySystem.Identity.Domain;

namespace HrAgencySystem.Api.Endpoints;

internal static class RouteGroupBuilderExtensions
{
    internal static RouteGroupBuilder WithOwnerRole(this RouteGroupBuilder builder)
    {
        builder.RequireAuthorization(opt =>
        {
            opt.RequireRole([nameof(PlatformRole.Owner)]);
        });

        return builder;
    }
}