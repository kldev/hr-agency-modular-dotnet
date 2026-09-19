namespace HrAgencySystem.Api.Endpoints.Teams;

public static class Endpoint
{
    public static void Map(this IEndpointRouteBuilder endpoints)
    {
        // No role gate on purpose: managing teams is open to any authenticated member of the
        // organization. The fallback-deny policy already keeps anonymous callers out.
        var group = endpoints.MapGroup("").WithTags("Teams");

        Maps.MapCreate.Map(group);
        Maps.MapGetSlice.Map(group);
        Maps.MapGet.Map(group);
        Maps.MapRename.Map(group);
        Maps.MapAddMember.Map(group);
        Maps.MapRemoveMember.Map(group);
        Maps.MapChangeMemberRole.Map(group);
    }
}
