namespace HrAgencySystem.Api.Endpoints.Suggestion;

internal static class Endpoint
{
    internal static void Map(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("").WithTags("Suggestion").RequireAuthorization();
        Maps.MapCompanies.Map(group);
        Maps.MapCompany.Map(group);
        Maps.MapUsers.Map(group);
        Maps.MapUser.Map(group);
        Maps.MapTags.Map(group);
        Maps.MapCompanyContacts.Map(group);
        Maps.MapJobPosts.Map(group);
    }
}