namespace HrAgencySystem.Api.Endpoints.Project;

public static class Endpoint
{
    public static void Map(this IEndpointRouteBuilder endpoints)
    {
        // No role gate: running a project is open to any authenticated member of the organization.
        // The fallback-deny policy already keeps anonymous callers out.
        var group = endpoints.MapGroup("").WithTags("Projects");

        Maps.MapCreate.Map(group);
        Maps.MapGetSlice.Map(group);
        Maps.MapGet.Map(group);
        Maps.MapUpdate.Map(group);
        Maps.MapChangeStatus.Map(group);
        Maps.MapAssignTeam.Map(group);
        Maps.MapAssignContact.Map(group);
        Maps.MapRemoveContact.Map(group);
        Maps.MapSetEmails.Map(group);
        Maps.MapRecordContract.Map(group);
        Maps.MapChangeContractStatus.Map(group);
        Maps.MapRecordCompliance.Map(group);
        Maps.MapComplianceCatalogue.Map(group);
    }
}
