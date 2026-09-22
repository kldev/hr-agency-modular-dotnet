namespace HrAgencySystem.Api.Endpoints.OrgStructure;

public static class Endpoint
{
    public static void Map(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("").WithTags("Agency - Structure");

        Maps.MapGet.Map(group);
        Maps.MapCreateUnit.Map(group);
        Maps.MapRenameUnit.Map(group);
        Maps.MapMoveUnit.Map(group);
        Maps.MapArchiveUnit.Map(group);
        Maps.MapAssignHead.Map(group);
        Maps.MapClearHead.Map(group);
        Maps.MapAddMember.Map(group);
        Maps.MapRemoveMember.Map(group);
        Maps.MapGetSupervisor.Map(group);
        Maps.MapGetSubordinates.Map(group);
    }
}
