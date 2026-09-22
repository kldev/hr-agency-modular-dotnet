using HrAgencySystem.Api.Infrastructure.OpenApi;
namespace HrAgencySystem.Api.Endpoints.Suggestion;

internal static class Endpoint
{
    internal static void Map(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("").WithTags(ApiTags.Suggestion).RequireAuthorization();
        Maps.MapCompanies.Map(group);
        Maps.MapCompany.Map(group);
        Maps.MapUsers.Map(group);
        Maps.MapUser.Map(group);
        Maps.MapTags.Map(group);
        Maps.MapCompanyContacts.Map(group);
        Maps.MapJobPosts.Map(group);
        Maps.MapTeams.Map(group);
        Maps.MapProjects.Map(group);
        Maps.MapProject.Map(group);
        Maps.MapPositions.Map(group);
        Maps.MapPosition.Map(group);
        Maps.MapTeam.Map(group);
        Maps.MapWorkers.Map(group);
        Maps.MapWorker.Map(group);
    }
}
